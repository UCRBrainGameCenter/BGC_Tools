using System;
using System.Collections.Generic;
using UnityEngine;

namespace BGC.Parameters.View
{
    public class WidgetFactoryContainerOrganizer : MonoBehaviour
    {
        [NonSerialized]
        public int slots = 0;
        [NonSerialized]
        public WidgetFactory.ContainerConfig config = WidgetFactory.ContainerConfig.None;
        [NonSerialized]
        public int childCount = 0;

        public bool CheckFlag(WidgetFactory.ContainerConfig flag)
        {
            return WidgetFactory.CheckFlag(config, flag);
        }
    }
}
