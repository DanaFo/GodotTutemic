using Godot;
using System;

[Tool]
public partial class ScenePostImport : EditorScenePostImport
{
    public override GodotObject _PostImport(Node scene)
    {
        GD.Print("postimport starting");
        try
        {
            RecursiveCreateCollision(scene);
            GD.Print("trying recursivecreate collision");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error during post import: {ex.Message}\n{ex.StackTrace}");
        }
        GD.Print("returning scene");
        return scene;
    }

    private void RecursiveCreateCollision(Node node)
    {
        GD.Print("recursive create collision starting...");
        try
        {
            if (node is MeshInstance3D meshInstance && meshInstance.Mesh != null)
            {
                meshInstance.CreateTrimeshCollision();
                GD.Print("trying create trimesh collision");
            }

            foreach (Node child in node.GetChildren())
            {
                GD.Print("recursion child node");
                RecursiveCreateCollision(child);
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error processing node {node.Name}: {ex.Message}");
        }
    }
}