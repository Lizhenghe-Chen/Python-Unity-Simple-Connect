using DG.Tweening;
using UnityEngine;

public class CubeMover : MonoBehaviour
{
    public void MoveCube(Vector3 position)
    {
        //move the cube to the new position
        transform.DOMove(position, 1f);
    }
}