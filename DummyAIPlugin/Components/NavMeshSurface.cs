using DummyAIPlugin.Navigation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace DummyAIPlugin.Components;

/// <summary>
/// Component responsible for NavMesh generation.
/// </summary>
public class NavMeshSurface : MonoBehaviour
{
    /// <summary>
    /// Maps a vector to its absolute version.
    /// </summary>
    /// <param name="value">Vector to map.</param>
    /// <returns>Absolute version of original vector.</returns>
    private static Vector3 Abs(Vector3 value) => new(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));

    /// <summary>
    /// Retireves world bounds.
    /// </summary>
    /// <param name="mat">Local to world coordinates matrix.</param>
    /// <param name="bounds">Bounds used for location and scaling.</param>
    /// <returns>World bounds.</returns>
    private static Bounds GetWorldBounds(Matrix4x4 mat, Bounds bounds)
    {
        var absAxisX = Abs(mat.MultiplyVector(Vector3.right));
        var absAxisY = Abs(mat.MultiplyVector(Vector3.up));
        var absAxisZ = Abs(mat.MultiplyVector(Vector3.forward));
        var worldPosition = mat.MultiplyPoint(bounds.center);
        var worldSize = absAxisX * bounds.size.x + absAxisY * bounds.size.y + absAxisZ * bounds.size.z;
        return new(worldPosition, worldSize);
    }

    /// <summary>
    /// Contains id of supported agent type.
    /// </summary>
    public int AgentTypeID { get; set; } = 0;

    /// <summary>
    /// Contains NavMesh sources collection mode used for generation.
    /// </summary>
    public CollectObjects CollectObjects { get; set; } = CollectObjects.All;

    /// <summary>
    /// Defines world bounds size.
    /// </summary>
    public Vector3 Size { get; set; } = new(10.0f, 10.0f, 10.0f);

    /// <summary>
    /// Defines world bounds center position.
    /// </summary>
    public Vector3 Center { get; set; } = new(0.0f, 2.0f, 0.0f);

    /// <summary>
    /// Contains collisions layer mask used for generation.
    /// </summary>
    public LayerMask LayerMask { get; set; } = new();

    /// <summary>
    /// Contains NavMesh geometry collection mode used for generation.
    /// </summary>
    public NavMeshCollectGeometry UseGeometry { get; set; } = NavMeshCollectGeometry.RenderMeshes;

    /// <summary>
    /// Contains default area value used for generation.
    /// </summary>
    public int DefaultArea { get; set; } = 0;

    /// <summary>
    /// Whether or not should <see cref="NavMeshAgent" /> instances be ignored during generation.
    /// </summary>
    public bool IgnoreNavMeshAgent { get; set; } = true;

    /// <summary>
    /// Whether or not should <see cref="NavMeshObstacle" /> instances be ignored during generation.
    /// </summary>
    public bool IgnoreNavMeshObstacle { get; set; } = true;

    /// <summary>
    /// Whether or not should the tile size be overridden during generation.
    /// </summary>
    public bool OverrideTileSize { get; set; } = false;

    /// <summary>
    /// Contains the tile size value used for override during generation.
    /// </summary>
    public int TileSize { get; set; } = 256;

    /// <summary>
    /// Whether or not should voxel size be overridden during generation.
    /// </summary>
    public bool OverrideVoxelSize { get; set; } = false;

    /// <summary>
    /// Contains the voxel size value used for override during generation.
    /// </summary>
    public float VoxelSize { get; set; } = 0.0f;

    /// <summary>
    /// Stores generated NavMesh data.
    /// </summary>
    public NavMeshData? NavMeshData { get; set; } = null;

    /// <summary>
    /// Stores additional generated NavMesh data.
    /// </summary>
    private NavMeshDataInstance _navMeshDataInstance = new();

    /// <summary>
    /// Contains last known position of game object.
    /// </summary>
    private Vector3 _lastPosition = Vector3.zero;

    /// <summary>
    /// Contains last known rotation of game object.
    /// </summary>
    private Quaternion _lastRotation = Quaternion.identity;

    /// <summary>
    /// Triggered when the component is enabled.
    /// </summary>
    public void OnEnable()
    {
        NavMesh.onPreUpdate += UpdateDataIfTransformChanged;
        AddData();
    }

    /// <summary>
    /// Triggered when the component is disabled.
    /// </summary>
    public void OnDisable()
    {
        RemoveData();
        NavMesh.onPreUpdate -= UpdateDataIfTransformChanged;
    }

    /// <summary>
    /// Adds additional data to generated NavMesh.
    /// </summary>
    public void AddData()
    {
        if (_navMeshDataInstance.valid)
        {
            return;
        }

        if (NavMeshData is not null)
        {
            _navMeshDataInstance = NavMesh.AddNavMeshData(NavMeshData, transform.position, transform.rotation);
            _navMeshDataInstance.owner = this;
        }

        _lastPosition = transform.position;
        _lastRotation = transform.rotation;
    }

    /// <summary>
    /// Removes additional data from generated NavMesh.
    /// </summary>
    public void RemoveData()
    {
        _navMeshDataInstance.Remove();
        _navMeshDataInstance = new();
    }

    /// <summary>
    /// Loads build settings used for generation.
    /// </summary>
    /// <returns>Loaded build settings.</returns>
    public NavMeshBuildSettings GetBuildSettings()
    {
        var buildSettings = NavMesh.GetSettingsByID(AgentTypeID);

        if (buildSettings.agentTypeID == -1)
        {
            LabApi.Features.Console.Logger.Warn($"No build settings for agent type ID {AgentTypeID}");
            buildSettings.agentTypeID = AgentTypeID;
        }

        if (OverrideTileSize)
        {
            buildSettings.overrideTileSize = true;
            buildSettings.tileSize = TileSize;
        }

        if (OverrideVoxelSize)
        {
            buildSettings.overrideVoxelSize = true;
            buildSettings.voxelSize = VoxelSize;
        }

        return buildSettings;
    }

    /// <summary>
    /// Builds new NavMesh.
    /// </summary>
    public void BuildNavMesh()
    {
        var sources = CollectSources();
        var sourcesBounds = CollectObjects == CollectObjects.Volume ? new Bounds(Center, Abs(Size)) : CalculateWorldBounds(sources);
        var data = NavMeshBuilder.BuildNavMeshData(GetBuildSettings(), sources, sourcesBounds, transform.position, transform.rotation);

        if (data is null)
        {
            return;
        }

        data.name = gameObject.name;
        RemoveData();
        NavMeshData = data;

        if (isActiveAndEnabled)
        {
            AddData();
        }
    }

    /// <summary>
    /// Updates data for generated NavMesh.
    /// </summary>
    /// <param name="data">NavMesh data to apply in update.</param>
    /// <returns>Asynchronous operation reference.</returns>
    public AsyncOperation UpdateNavMesh(NavMeshData data)
    {
        var sources = CollectSources();
        var sourcesBounds = CollectObjects == CollectObjects.Volume ? new Bounds(Center, Abs(Size)) : CalculateWorldBounds(sources);
        return NavMeshBuilder.UpdateNavMeshDataAsync(data, GetBuildSettings(), sources, sourcesBounds);
    }

    /// <summary>
    /// Collects all valid source objects for NavMesh generation.
    /// </summary>
    /// <returns>List of source objects to use in generation.</returns>
    private List<NavMeshBuildSource> CollectSources()
    {
        var sources = new List<NavMeshBuildSource>();

        switch (CollectObjects)
        {
            case CollectObjects.All:
                NavMeshBuilder.CollectSources(null, LayerMask, UseGeometry, DefaultArea, [], sources);
                break;
            case CollectObjects.Children:
                NavMeshBuilder.CollectSources(transform, LayerMask, UseGeometry, DefaultArea, [], sources);
                break;
            default:
                var localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                var worldBounds = GetWorldBounds(localToWorld, new(Center, Size));
                NavMeshBuilder.CollectSources(worldBounds, LayerMask, UseGeometry, DefaultArea, [], sources);
                break;
        }

        if (IgnoreNavMeshAgent)
        {
            sources.RemoveAll(static x => x.component?.gameObject.GetComponent<NavMeshAgent>() is not null);
        }

        if (IgnoreNavMeshObstacle)
        {
            sources.RemoveAll(static x => x.component?.gameObject.GetComponent<NavMeshObstacle>() is not null);
        }

        return sources;
    }

    /// <summary>
    /// Calculates world bounds for provided sources.
    /// </summary>
    /// <param name="sources">Source objects to encapsulate.</param>
    /// <returns>New bounds encapsulating all provided sources.</returns>
    private Bounds CalculateWorldBounds(List<NavMeshBuildSource> sources)
    {
        var worldToLocal = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        worldToLocal = worldToLocal.inverse;
        var result = new Bounds();

        foreach (var src in sources)
        {
            switch (src.shape)
            {
                case NavMeshBuildSourceShape.Mesh:
                    result.Encapsulate(GetWorldBounds(worldToLocal * src.transform,
                        src.sourceObject is not Mesh mesh ? new(Vector3.zero, src.size) : mesh.bounds));
                    break;
                case NavMeshBuildSourceShape.Terrain:
                    result.Encapsulate(GetWorldBounds(worldToLocal * src.transform,
                        src.sourceObject is not TerrainData terrain ? new(Vector3.zero, src.size) : new(0.5f * terrain.size, terrain.size)));
                    break;
                default:
                    result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new(Vector3.zero, src.size)));
                    break;
            }
        }

        result.Expand(0.1f);
        return result;
    }

    /// <summary>
    /// Performs data update if position or rotation changed since last check.
    /// </summary>
    private void UpdateDataIfTransformChanged()
    {
        if (_lastPosition != transform.position || _lastRotation != transform.rotation)
        {
            RemoveData();
            AddData();
        }
    }
}
