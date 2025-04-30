using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextOutline : MonoBehaviour
{
    [SerializeField] private Color color = Color.black;
    [SerializeField] private float width = 0.1f;

    private void Start()
    {
        var text = GetComponent<TMP_Text>();
        text.fontMaterial.SetColor("_OutlineColor", color);
        text.fontMaterial.SetFloat("_OutlineWidth", width);
    }
}