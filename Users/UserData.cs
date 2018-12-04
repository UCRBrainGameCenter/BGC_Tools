using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace BGC.Users
{
    public class UserData : ProfileData
    {
        public override bool IsDefault => false;

        //Constructor
        public UserData(string userName)
            : base(userName)
        {
        }

    }
}

