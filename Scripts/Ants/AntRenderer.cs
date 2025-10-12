using Godot;
using System;

public partial class AntRenderer : Node2D
{
    [Export] public Texture2D AntTexture;
    [Export] private ShaderMaterial _shaderMaterial;
    private MultiMesh _multiMesh;
    private MultiMeshInstance2D _multiMeshInstance;
    private Hive _hive;

    public int AntCount;

    public void Init(Hive hive)
    {
        _hive = hive;

        AntCount = _hive.GetAntCount();

        // Create MultiMesh
        _multiMesh = new MultiMesh();
        _multiMesh.InstanceCount = AntCount;

        // Use a small quad as the base mesh (size matches sprite pixels or world units)
        var quad = new QuadMesh();
        quad.Size = new Vector2(2f, 2f);
        _multiMesh.Mesh = quad;

        // Create a shader material that simply samples the texture
        _shaderMaterial.SetShaderParameter("tex", AntTexture);

        // Create and configure the MultiMeshInstance2D
        _multiMeshInstance = new MultiMeshInstance2D();
        _multiMeshInstance.Multimesh = _multiMesh;
        _multiMeshInstance.Material = _shaderMaterial;

        AddChild(_multiMeshInstance);
    }

    public override void _Process(double delta)
    {
        UpdateAnts(_hive.GetAnts());
    }


    public void UpdateAnts(Span<Ant> ants)
    {
        int length = ants.Length;
        for (int i = 0; i < length; i++)
        {
            ref Ant a = ref ants[i];
            float rotation = Mathf.Atan2(a.Velocity.Y, a.Velocity.X);
            _multiMesh.SetInstanceTransform2D(i, new Transform2D(rotation, a.Pos));
        }
    }


}
