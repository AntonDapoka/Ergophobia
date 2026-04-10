using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WriteBlockScript : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private string str;



    public string GetText()
    {
        return inputField.text;
    }


}
