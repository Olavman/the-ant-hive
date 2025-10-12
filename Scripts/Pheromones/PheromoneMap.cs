using Godot;
using System;

// Visual representation of pheromones
public partial class PheromoneMap : Node
{
    public PheromoneGrid Grid;
    private ImageTexture pheromoneTexture;
    private Image pheromoneImage;
    private bool _isVisible = true;


    public void Init(PheromoneGrid grid)
    {
        Grid = grid;
    }

    public override void _Ready()
    {
        //Grid._Ready();
        if (Grid == null) GD.Print("No Grid");
        Grid.PheromonesUpdated += OnPheromonesUpdated;


        int width = Grid.Width;
        int height = Grid.Height;

        // Create an Image with the same dimensions as the grid
        pheromoneImage = Image.CreateEmpty(width, height, false, Image.Format.Rgba8);
        pheromoneTexture = ImageTexture.CreateFromImage(pheromoneImage);

        UpdatePheromoneTexture();
    }

    private void OnPheromonesUpdated()
    {
        if (!_isVisible) return;
        UpdatePheromoneTexture();
    }

    public void ToggleVisibility()
    {
        _isVisible = !_isVisible;

        if (_isVisible) UpdatePheromoneTexture();
    }


    public void UpdatePheromoneTexture()
    {
        int width = Grid.Width;
        int height = Grid.Height;

        byte[] pixels = new byte[width * height * 4];
        int amplification = 3000;

        // Write grid data to image
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float colony = Grid.GetPheromoneLevel(x, y, PHEROMONE_TYPE.COLONY);
                float search = Grid.GetPheromoneLevel(x, y, PHEROMONE_TYPE.SEARCHING);
                float returning = Grid.GetPheromoneLevel(x, y, PHEROMONE_TYPE.RETURNING);
                float alarm = Grid.GetPheromoneLevel(x, y, PHEROMONE_TYPE.ALARM);

                // Combine all pheromones into RGBA channels
                int index = (y * width + x) * 4;
                pixels[index + 0] = (byte)(Mathf.Clamp(alarm + colony * 0.5f, 0, 1) * amplification);
                pixels[index + 1] = (byte)(Mathf.Clamp(returning, 0, 1) * amplification);
                pixels[index + 2] = (byte)(Mathf.Clamp(search + colony * 0.5f, 0, 1) * amplification);
                //pixels[3] = 255;
                // Calculate total pheromone strength for alpha
                float totalStrength = alarm + search + returning + colony;
                pixels[index + 3] = (byte)(Mathf.Clamp(totalStrength, 0, 1) * amplification);
            }
        }

        // Update the texture
        pheromoneImage.SetData(width, height, false, Image.Format.Rgba8, pixels);
        pheromoneTexture.Update(pheromoneImage);

        // Pass to shader material
        var material = GetNode<TextureRect>("Sprite2D").Material as ShaderMaterial;
        material.SetShaderParameter("pheromone_map", pheromoneTexture);
    }

    public override void _Process(double delta)
    {
        int loops = (Grid.Width * Grid.Height) / 10;
        Grid.DiffuseGridSlow(loops);
    }
}
