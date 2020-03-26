using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ambiens.avrs.componentserializer;
using ambiens.avrs.model;
using ambiens.avrs.controller;
using System;



public class BimDataComponent : AUnityComponentSerializer<BimData>
{
    public BimDataComponent(CScene controller) : base(controller)
    {

    }

    public override void fromModel(BimData r, MComponentBase model)
    {
        r.dataType = (BimData.BimDataType)Enum.Parse(typeof(BimData.BimDataType), model.v[0]);

        // Remove Bim Type
        model.k.RemoveAt(0);
        model.v.RemoveAt(0);
        
        // Read the dictionary
        for (int i = 0; i < model.k.Count; i++)
        {
            r.bimDatas.Add(model.k[i],model.v[i]);
        }
    }

    public override string GetComponentName()
    {
        return "BimData";
    }

    public override List<string> GetDeserializationID()
    {
        var l = this.GetBehaviourDeserializationID();
        l.Add(GetComponentName());
        return l;
    }

    public override void ManageComponentBeforeSerialization(Component comp)
    {
        throw new NotImplementedException();
    }

    public override Dictionary<string, string> toDictionary(BimData r)
    {
        return r.bimDatas;
    }

}
