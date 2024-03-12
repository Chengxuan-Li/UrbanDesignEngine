using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DotRecast;
using DotRecast.Core;
using DotRecast.Recast;
using DotRecast.Detour;


using System.IO;
using DotRecast.Core.Numerics;
using DotRecast.Recast.Geom;

using TriangleNet.Meshing;

using TriangleNet.Geometry;

namespace UrbanDesignEngine.Triangulation
{

    
    public class NavMesher
    {
        RecastSoloMeshTest rsmt;
        public NavMesher(IMesh mesh)
        {
            rsmt = new RecastSoloMeshTest();
            TriangleNetParser.ToDotRecastVerticesFaces(mesh, out float[] vs, out int[] fs);
            rsmt.TestBuild(vs, fs);

            rsmt.SetUp();
        }

        public void Path(List<RcVec3f> startPoss, List<RcVec3f> endPoss, out List<List<long>> paths, out List<List<DtStraightPath>> sPaths)
        {
            List<long> sis = new List<long>();
            List<long> eis = new List<long>();
            List<RcVec3f> sps = new List<RcVec3f>();
            List<RcVec3f> eps = new List<RcVec3f>();

            foreach (var sp in startPoss)
            {
                rsmt.FindNearest(sp, out long nref, out var npt, out bool iop);
                sis.Add(nref);
                sps.Add(npt);
            }

            foreach (var ep in endPoss)
            {
                rsmt.FindNearest(ep, out long nref, out var npt, out bool iop);
                eis.Add(nref);
                eps.Add(npt);
            }

            rsmt.TestFindPath(sis, eis, sps, eps, out paths, out sPaths);
        }
    }



    public class RecastSoloMeshTest
    {
        private DtMeshData meshData;

        private const float m_cellSize = 0.3f;
        private const float m_cellHeight = 99.9f;
        private const float m_agentHeight = 2.0f;
        private const float m_agentRadius = 0.6f;
        private const float m_agentMaxClimb = 0.9f;
        private const float m_agentMaxSlope = 45.0f;
        private const int m_regionMinSize = 2;
        private const int m_regionMergeSize = 10;
        private const float m_edgeMaxLen = 12.0f;
        private const float m_edgeMaxError = 1.3f;
        private const int m_vertsPerPoly = 6;
        private const float m_detailSampleDist = 6.0f;
        private const float m_detailSampleMaxError = 1.0f;
        private RcPartition m_partitionType = RcPartition.WATERSHED;

        public void TestBuild(float[] vertices, int[] faces)
        {
            // RcPartition partitionType, int expDistance, int expRegions,
            // int expContours, int expVerts, int expPolys, int expDetMeshes, int expDetVerts, int expDetTris
            // m_partitionType = partitionType;
            SimpleInputGeomProvider geomProvider = new SimpleInputGeomProvider(vertices, faces);
            long time = RcFrequency.Ticks;
            RcVec3f bmin = geomProvider.GetMeshBoundsMin();
            RcVec3f bmax = geomProvider.GetMeshBoundsMax();
            bmax.Z = 999;
            RcContext m_ctx = new RcContext();
            //
            // Step 1. Initialize build config.
            //

            // Init build configuration from GUI
            RcConfig cfg = new RcConfig(
                m_partitionType,
                m_cellSize, m_cellHeight,
                m_agentMaxSlope, m_agentHeight, m_agentRadius, m_agentMaxClimb,
                m_regionMinSize, m_regionMergeSize,
                m_edgeMaxLen, m_edgeMaxError,
                m_vertsPerPoly,
                m_detailSampleDist, m_detailSampleMaxError,
                true, true, true,
                SampleAreaModifications.SAMPLE_AREAMOD_ALL, true);
            RcBuilderConfig bcfg = new RcBuilderConfig(cfg, bmin, bmax);

            //
            // Step 2. Rasterize input polygon soup.
            //

            // Allocate voxel heightfield where we rasterize our input data to.
            RcHeightfield m_solid = new RcHeightfield(bcfg.width, bcfg.height, bcfg.bmin, bcfg.bmax, cfg.Cs, cfg.Ch, cfg.BorderSize);

            foreach (RcTriMesh geom in geomProvider.Meshes())
            {
                float[] verts = geom.GetVerts();
                int[] tris = geom.GetTris();
                int ntris = tris.Length / 3;

                // Allocate array that can hold triangle area types.
                // If you have multiple meshes you need to process, allocate
                // and array which can hold the max number of triangles you need to process.

                // Find triangles which are walkable based on their slope and rasterize them.
                // If your input data is multiple meshes, you can transform them here, calculate
                // the are type for each of the meshes and rasterize them.
                int[] m_triareas = RcCommons.MarkWalkableTriangles(m_ctx, cfg.WalkableSlopeAngle, verts, tris, ntris, cfg.WalkableAreaMod);
                RcRasterizations.RasterizeTriangles(m_ctx, verts, tris, m_triareas, ntris, m_solid, cfg.WalkableClimb);
            }

            //
            // Step 3. Filter walkable surfaces.
            //

            // Once all geometry is rasterized, we do initial pass of filtering to
            // remove unwanted overhangs caused by the conservative rasterization
            // as well as filter spans where the character cannot possibly stand.

        
            //RcFilters.FilterLowHangingWalkableObstacles(m_ctx, cfg.WalkableClimb, m_solid);
            //RcFilters.FilterLedgeSpans(m_ctx, cfg.WalkableHeight, cfg.WalkableClimb, m_solid);
            //RcFilters.FilterWalkableLowHeightSpans(m_ctx, cfg.WalkableHeight, m_solid);
        

            //
            // Step 4. Partition walkable surface to simple regions.
            //

            // Compact the heightfield so that it is faster to handle from now on.
            // This will result more cache coherent data as well as the neighbours
            // between walkable cells will be calculated.
            RcCompactHeightfield m_chf = RcCompacts.BuildCompactHeightfield(m_ctx, cfg.WalkableHeight, cfg.WalkableClimb, m_solid);

            // Erode the walkable area by agent radius.
            RcAreas.ErodeWalkableArea(m_ctx, cfg.WalkableRadius, m_chf);

            // (Optional) Mark areas.
            /*
             * ConvexVolume vols = m_geom->GetConvexVolumes(); for (int i = 0; i < m_geom->GetConvexVolumeCount(); ++i)
             * RcMarkConvexPolyArea(m_ctx, vols[i].verts, vols[i].nverts, vols[i].hmin, vols[i].hmax, (unsigned
             * char)vols[i].area, *m_chf);
             */

            // Partition the heightfield so that we can use simple algorithm later
            // to triangulate the walkable areas.
            // There are 3 partitioning methods, each with some pros and cons:
            // 1) Watershed partitioning
            // - the classic Recast partitioning
            // - creates the nicest tessellation
            // - usually slowest
            // - partitions the heightfield into nice regions without holes or
            // overlaps
            // - the are some corner cases where this method creates produces holes
            // and overlaps
            // - holes may appear when a small obstacles is close to large open area
            // (triangulation can handle this)
            // - overlaps may occur if you have narrow spiral corridors (i.e
            // stairs), this make triangulation to fail
            // * generally the best choice if you precompute the navmesh, use this
            // if you have large open areas
            // 2) Monotone partioning
            // - fastest
            // - partitions the heightfield into regions without holes and overlaps
            // (guaranteed)
            // - creates long thin polygons, which sometimes causes paths with
            // detours
            // * use this if you want fast navmesh generation
            // 3) Layer partitoining
            // - quite fast
            // - partitions the heighfield into non-overlapping regions
            // - relies on the triangulation code to cope with holes (thus slower
            // than monotone partitioning)
            // - produces better triangles than monotone partitioning
            // - does not have the corner cases of watershed partitioning
            // - can be slow and create a bit ugly tessellation (still better than
            // monotone)
            // if you have large open areas with small obstacles (not a problem if
            // you use tiles)
            // * good choice to use for tiled navmesh with medium and small sized
            // tiles
            //long time3 = RcFrequency.Ticks;

            if (m_partitionType == RcPartition.WATERSHED)
            {
                // Prepare for region partitioning, by calculating distance field
                // along the walkable surface.
                RcRegions.BuildDistanceField(m_ctx, m_chf);
                // Partition the walkable surface into simple regions without holes.
                RcRegions.BuildRegions(m_ctx, m_chf, cfg.MinRegionArea, cfg.MergeRegionArea);
            }
            else if (m_partitionType == RcPartition.MONOTONE)
            {
                // Partition the walkable surface into simple regions without holes.
                // Monotone partitioning does not need distancefield.
                RcRegions.BuildRegionsMonotone(m_ctx, m_chf, cfg.MinRegionArea, cfg.MergeRegionArea);
            }
            else
            {
                // Partition the walkable surface into simple regions without holes.
                RcRegions.BuildLayerRegions(m_ctx, m_chf, cfg.MinRegionArea);
            }

            //Assert.That(m_chf.maxDistance, Is.EqualTo(expDistance), "maxDistance");
            //Assert.That(m_chf.maxRegions, Is.EqualTo(expRegions), "Regions");
            //
            // Step 5. Trace and simplify region contours.
            //

            // Create contours.
            RcContourSet m_cset = RcContours.BuildContours(m_ctx, m_chf, cfg.MaxSimplificationError, cfg.MaxEdgeLen, RcBuildContoursFlags.RC_CONTOUR_TESS_AREA_EDGES);

            //Assert.That(m_cset.conts.Count, Is.EqualTo(expContours), "Contours");
            //
            // Step 6. Build polygons mesh from contours.
            //

            // Build polygon navmesh from the contours.
            RcPolyMesh m_pmesh = RcMeshs.BuildPolyMesh(m_ctx, m_cset, cfg.MaxVertsPerPoly);
            //Assert.That(m_pmesh.nverts, Is.EqualTo(expVerts), "Mesh Verts");
            //Assert.That(m_pmesh.npolys, Is.EqualTo(expPolys), "Mesh Polys");
            //
            // Step 7. Create detail mesh which allows to access approximate height
            // on each polygon.
            //

            RcPolyMeshDetail m_dmesh = RcMeshDetails.BuildPolyMeshDetail(m_ctx, m_pmesh, m_chf, cfg.DetailSampleDist,
                cfg.DetailSampleMaxError);

            //Assert.That(m_dmesh.nmeshes, Is.EqualTo(expDetMeshes), "Mesh Detail Meshes");
            //Assert.That(m_dmesh.nverts, Is.EqualTo(expDetVerts), "Mesh Detail Verts");
            //Assert.That(m_dmesh.ntris, Is.EqualTo(expDetTris), "Mesh Detail Tris");
            //long time2 = RcFrequency.Ticks;
            //Console.WriteLine(filename + " : " + partitionType + "  " + (time2 - time) / TimeSpan.TicksPerMillisecond + " ms");
            //Console.WriteLine("           " + (time3 - time) / TimeSpan.TicksPerMillisecond + " ms");
            //SaveObj(filename.Substring(0, filename.LastIndexOf('.')) + "_" + partitionType + "_detail.obj", m_dmesh);
            //SaveObj(filename.Substring(0, filename.LastIndexOf('.')) + "_" + partitionType + ".obj", m_pmesh);
            //foreach (var rtt in m_ctx.ToList())
            //{
            //    Console.WriteLine($"{rtt.Key} : {rtt.Millis} ms");
            //}

            DtNavMeshCreateParams option = new DtNavMeshCreateParams();
            option.verts = m_pmesh.verts;
            option.vertCount = m_pmesh.nverts;
            option.polys = m_pmesh.polys;
            option.polyAreas = m_pmesh.areas;
            option.polyFlags = m_pmesh.flags;
            option.polyCount = m_pmesh.npolys;
            option.nvp = m_pmesh.nvp;
            option.detailMeshes = m_dmesh.meshes;
            option.detailVerts = m_dmesh.verts;
            option.detailVertsCount = m_dmesh.nverts;
            option.detailTris = m_dmesh.tris;
            option.detailTriCount = m_dmesh.ntris;
            option.walkableHeight = m_agentHeight;
            option.walkableRadius = m_agentRadius;
            option.walkableClimb = m_agentMaxClimb;
            option.bmin = m_pmesh.bmin;
            option.bmax = m_pmesh.bmax;
            option.cs = m_cellSize;
            option.ch = m_cellHeight;
            option.buildBvTree = true;

            option.offMeshConVerts = new float[6];
            option.offMeshConVerts[0] = 0.1f;
            option.offMeshConVerts[1] = 0.2f;
            option.offMeshConVerts[2] = 0.3f;
            option.offMeshConVerts[3] = 0.4f;
            option.offMeshConVerts[4] = 0.5f;
            option.offMeshConVerts[5] = 0.6f;
            option.offMeshConRad = new float[1];
            option.offMeshConRad[0] = 0.1f;
            option.offMeshConDir = new int[1];
            option.offMeshConDir[0] = 1;
            option.offMeshConAreas = new int[1];
            option.offMeshConAreas[0] = 2;
            option.offMeshConFlags = new int[1];
            option.offMeshConFlags[0] = 12;
            option.offMeshConUserID = new int[1];
            option.offMeshConUserID[0] = 0x4567;
            option.offMeshConCount = 1;
            meshData = DtNavMeshBuilder.CreateNavMeshData(option);

        }

        /*
        protected static readonly long[] startRefs =
{
        281474976710696L, 281474976710773L, 281474976710680L, 281474976710753L, 281474976710733L
    };

        protected static readonly long[] endRefs =
        {
        281474976710721L, 281474976710767L, 281474976710758L, 281474976710731L, 281474976710772L
    };

        protected static readonly RcVec3f[] startPoss =
        {
        new RcVec3f(22.60652f, 10.197294f, -45.918674f),
        new RcVec3f(22.331268f, 10.197294f, -1.0401875f),
        new RcVec3f(18.694363f, 15.803535f, -73.090416f),
        new RcVec3f(0.7453353f, 10.197294f, -5.94005f),
        new RcVec3f(-20.651257f, 5.904126f, -13.712508f)
    };

        protected static readonly RcVec3f[] endPoss =
        {
        new RcVec3f(6.4576626f, 10.197294f, -18.33406f),
        new RcVec3f(-5.8023443f, 0.19729415f, 3.008419f),
        new RcVec3f(38.423977f, 10.197294f, -0.116066754f),
        new RcVec3f(0.8635526f, 10.197294f, -10.31032f),
        new RcVec3f(18.784092f, 10.197294f, 3.0543678f),
    };
        */
        protected DtNavMeshQuery query;
        protected DtNavMesh navmesh;

        public void SetUp()
        {
            navmesh = CreateNavMesh();
            query = new DtNavMeshQuery(navmesh);
        }

        protected DtNavMesh CreateNavMesh()
        {
            
            return new DtNavMesh(meshData, 6, 0);
        }


        public DtStatus FindNearest(RcVec3f pos, out long nref, out RcVec3f npt, out bool iop)
        {
            return query.FindNearestPoly(pos, new RcVec3f(1.0f), new DtQueryDefaultFilter(), out nref, out npt, out iop);
        }

        public void TestFindPath(List<long> startRefs, List<long> endRefs, List<RcVec3f> startPoss, List<RcVec3f> endPoss, out List<List<long>> paths, out List<List<DtStraightPath>> straightPaths)
        {
            IDtQueryFilter filter = new DtQueryDefaultFilter();
            paths = new List<List<long>>();
            straightPaths = new List<List<DtStraightPath>>();

            for (int i = 0; i < startRefs.Count; i++)
            {
                // startRefs.Length; i++) {
                var path = new List<long>();
                long startRef = startRefs[i];
                long endRef = endRefs[i];
                var startPos = startPoss[i];
                var endPos = endPoss[i];
                var status = query.FindPath(startRef, endRef, startPos, endPos, filter, ref path, DtFindPathOption.NoOption);
                var straightPath = new List<DtStraightPath>();
                status = query.FindStraightPath(startPos, endPos, path, ref straightPath, int.MaxValue, 0);

                paths.Add(path.ToList());
                straightPaths.Add(straightPath.ToList());
                /*
                //Assert.That(straightPath.Count, Is.EqualTo(STRAIGHT_PATHS[i].Length));
                for (int j = 0; j < STRAIGHT_PATHS[i].Length; j++)
                {
                    Assert.That(straightPath[j].refs, Is.EqualTo(STRAIGHT_PATHS[i][j].refs));
                    Assert.That(straightPath[j].pos.X, Is.EqualTo(STRAIGHT_PATHS[i][j].pos.X).Within(0.01f));
                    Assert.That(straightPath[j].pos.Y, Is.EqualTo(STRAIGHT_PATHS[i][j].pos.Y).Within(0.01f));
                    Assert.That(straightPath[j].pos.Z, Is.EqualTo(STRAIGHT_PATHS[i][j].pos.Z).Within(0.01f));
                    Assert.That(straightPath[j].flags, Is.EqualTo(STRAIGHT_PATHS[i][j].flags));
                }
                */
            }
        }

    }

}

