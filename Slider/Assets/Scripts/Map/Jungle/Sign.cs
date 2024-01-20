using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Sign : Box
{
    public RecipeList recipes;
    public Dictionary<Path, Shape> recievedShapes = new Dictionary<Path, Shape>();

    public Shape question;
    
    // Start is called before the first frame update
    void Awake()
    {
        SetPaths();
        foreach (Direction d in paths.Keys)
        {
            paths[d].ChangePair();
        }

        recievedShapes = new Dictionary<Path, Shape>();
        foreach (Direction d in paths.Keys)
        {
            recievedShapes.Add(paths[d], null);
        }
    }
    private void OnEnable()
    {
        SGridAnimator.OnSTileMoveStart += UpdateShapesOnTileMove;
        SGridAnimator.OnSTileMoveEnd += OnSTileMoveEnd;
        SGridAnimator.OnSTileMoveStart += DeactivatePathsOnSTileMove;
        SGrid.OnSTileEnabled += STileEnabled;
    }

    private void OnDisable()
    {
        SGridAnimator.OnSTileMoveStart -= UpdateShapesOnTileMove;
        SGridAnimator.OnSTileMoveEnd -= OnSTileMoveEnd;
        SGridAnimator.OnSTileMoveStart -= DeactivatePathsOnSTileMove;
        SGrid.OnSTileEnabled -= STileEnabled;
    }

    private void UpdateShapesOnTileMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        recievedShapes = new Dictionary<Path, Shape>();
        foreach (Direction d in paths.Keys)
        {
            recievedShapes.Add(paths[d], null);
        }
    }

    public override void RecieveShape(Path path, Shape shape, List<string> parents)
    {
/*        if (this.gameObject.name == "Sign 5.1")
        {
            if (shape == null)
            {
                print("recieve null from ");
            }
            else
            {
                print("recieve " + shape.name + " from ");
            }
            foreach (string parent in parents)
            {
                print(parent);
            }
        }*/

        // && (shape != null && this.currentShape != null)
        //this isn't working
        if (parents.Contains(this.gameObject.name))
        {
            return;
        }

        if (path.pair != null)
        {
            recievedShapes[path.pair] = shape;
            this.MergeShapes();
            this.CreateShape(parents);
        }
        else
        {
            recievedShapes[path] = shape;
            this.MergeShapes();
            this.CreateShape(parents);
        }
    }
    public void MergeShapes()
    {
        List<Shape> shapesRecieved = new List<Shape>();

        foreach (Direction d in paths.Keys)
        {
            if (recievedShapes[paths[d]] != null)
            {
                shapesRecieved.Add(recievedShapes[paths[d]]);
            }
        }
/*
        if (this.gameObject.name == "Sign 5.1")
        {
            print("recieved shapes");
            foreach (Shape shape in shapesRecieved)
            {
                print(shape.name);
            }
        }*/


        foreach (Recipe recipe in recipes.list)
        {
            Shape hold = recipe.Check(shapesRecieved);
            if (hold != null)
            {
                currentShape = hold;
                return;
            }
        }

        if (shapesRecieved.Count == 0)
        {
            currentShape = null;
        }
        else if (shapesRecieved.Count == 1)
        {
            currentShape = shapesRecieved[0];
        } else
        {
            currentShape = question;
        }
    }
}
