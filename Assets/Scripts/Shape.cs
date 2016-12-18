using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Shape : MonoBehaviour
{
    public BonusType Bonus { get; set; }
    public int Column { get; set; }
    public int Row { get; set; }

    public int Type { get; set; }

    public Shape()
    {
        Bonus = BonusType.None;
    }
    void Update()
    {
        ///destroy objects once they leave the screen
        if (transform.position.y <-100 && !GetComponent<Rigidbody2D>().isKinematic)
        {
            Destroy(this);
        }
    }

    public void eject()
    {
        GetComponent<Rigidbody2D>().isKinematic = false;
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<Rigidbody2D>().AddForceAtPosition(new Vector2(UnityEngine.Random.Range(-100,100), UnityEngine.Random.Range(20, 100)), new Vector2(UnityEngine.Random.Range(-5, 5), UnityEngine.Random.Range(-5, 5)));
        GetComponent<Rigidbody2D>().AddTorque(.2f);
    }

    /// <summary>
    /// Checks if the current shape is of the same type as the parameter
    /// </summary>
    /// <param name="otherShape"></param>
    /// <returns></returns>
    public bool IsSameType(Shape otherShape)
    {
        if (otherShape == null || !(otherShape is Shape))
            throw new ArgumentException("otherShape");

        return this.Type==otherShape.Type;
    }

    /// <summary>
    /// Constructor alternative
    /// </summary>
    /// <param name="type"></param>
    /// <param name="row"></param>
    /// <param name="column"></param>
    public void Assign(int type, int row, int column)
    {

        Column = column;
        Row = row;
        Type = type;
    }

    /// <summary>
    /// Swaps properties of the two shapes
    /// We could do a shallow copy/exchange here, but anyway...
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static void SwapColumnRow(Shape a, Shape b)
    {
        int temp = a.Row;
        a.Row = b.Row;
        b.Row = temp;

        temp = a.Column;
        a.Column = b.Column;
        b.Column = temp;
    }
}



