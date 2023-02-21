using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : Box
{
    public RecipeList recipes;
    public Dictionary<Path, Shape> recievedShapes = new Dictionary<Path, Shape>(); 
    
    // Start is called before the first frame update
    void Awake()
    {
        SetPaths();
        foreach (Direction d in paths.Keys)
        {
            paths[d].ChangePair();
        }

        if (recievedShapes.Count == 0)
        {
            foreach (Direction d in paths.Keys)
            {
                recievedShapes.Add(paths[d], null);
            }
        }
    }
    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveStart += UpdateShapesOnTileMove;
        SGridAnimator.OnSTileMoveStart += DeactivatePathsOnSTileMove;
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveStart -= UpdateShapesOnTileMove;
        SGridAnimator.OnSTileMoveStart -= DeactivatePathsOnSTileMove;
    }

    private void UpdateShapesOnTileMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        foreach (Direction d in paths.Keys)
        {
            paths[d].ChangePair();
        }
        //remove all shapes (happens after the bin pushes the triangle is there a way to time this
        recievedShapes = new Dictionary<Path, Shape>();
        foreach (Direction d in paths.Keys)
        {
            recievedShapes.Add(paths[d], null);
        }
    }


    public override void RecieveShape(Path path, Shape shape, List<Box> parents)
    {
        if (parents.Contains(this) && shape != null)
        {
            return;
        }

        parents.Add(this);

        if (path.pair != null)
        {
            recievedShapes[path.pair] = shape;
            MergeShapes();
            CreateShape(parents);
        }
        else
        {
            recievedShapes[path] = shape;
            MergeShapes();
            CreateShape(parents);
        }
    }
    public void MergeShapes()
    {
        //print("merging");
        List<Shape> shapesRecieved = new List<Shape>(); 
        foreach (Direction d in paths.Keys)
        {
            if (recievedShapes[paths[d]] != null)
            {
/*                print(recievedShapes[paths[d]]);
                print(d);*/
                shapesRecieved.Add(recievedShapes[paths[d]]);
            }
        }

/*        print("merging");
        foreach (Shape s in shapesRecieved)
        {
            print(s.name);
        }*/

        foreach (Recipe recipe in recipes.list)
        {
            //print(recipe.result.name);
            Shape hold = recipe.Check(shapesRecieved);
            if (hold != null)
            {
                currentShape = hold;
               // print("merged: " + currentShape.type);
                return;
            }
        }

        if (shapesRecieved.Count == 0)
        {
            currentShape = null;
        }
        else
        {
            currentShape = shapesRecieved[0];
        }
    }
}
