using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextFadeIn : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI _textMeshPro;
    [SerializeField]
    private string _text;
    private int _nCharacters;

    void Start()
    {
        _textMeshPro.text = _text;
        StartCoroutine(TextVisible());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator TextVisible()
    {
        if (_nCharacters >= _text.Length)
        {
            yield return new WaitForSeconds(4f);
            _textMeshPro.gameObject.active = false;
        
        }
        else
        {
            _nCharacters++;
            _textMeshPro.maxVisibleCharacters = _nCharacters;
            yield return new WaitForSeconds(0.02f);
            StartCoroutine(TextVisible());
        }


    }
}
