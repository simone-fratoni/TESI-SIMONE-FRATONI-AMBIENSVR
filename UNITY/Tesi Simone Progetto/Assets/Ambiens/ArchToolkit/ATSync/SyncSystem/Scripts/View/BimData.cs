using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
[ExecuteInEditMode]
public class BimData : MonoBehaviour
{
    public enum BimDataType
    {
        Object,
        Material
    }
    [SerializeField]
    public BimDataType dataType;
    
    public Dictionary<string, string> bimDatas = new Dictionary<string, string>();

    public List<string> keys = new List<string>();
    public List<string> values = new List<string>();
    
    private void Start()
    {
        foreach (var k in bimDatas.Keys.ToList())
        {
            keys.Add(k);
        }

        foreach (var v in bimDatas.Values.ToList())
        {
            values.Add(v);
        }
    }
}
