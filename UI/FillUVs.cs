using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BGC.UI
{
    public class FillUVs : BaseMeshEffect
    {
        List<UIVertex> vertList = new List<UIVertex>();

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
                return;

            vh.GetUIVertexStream(vertList);

            if (vertList.Count < 52)
            {
                vertList.Clear();
                return;
            }

            Vector3 min = vertList[0].position;
            Vector3 max = vertList[51].position;
            Vector3 diff = max - min;
            Vector3 adj;

            vh.Clear();

            for (int i = 0; i < vertList.Count; i++)
            {
                UIVertex vert = vertList[i];

                adj = vert.position - min;
                vert.uv1 = new Vector2(adj.x / diff.x, adj.y / diff.y);
                vertList[i] = vert;
            }

            vh.AddUIVertexTriangleStream(vertList);

            vertList.Clear();
        }
    }
}
