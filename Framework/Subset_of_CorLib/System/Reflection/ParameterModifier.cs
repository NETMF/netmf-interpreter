////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////namespace System.Reflection {
namespace System.Reflection {

    using System;
    public struct ParameterModifier {
        internal bool[] _byRef;
        public ParameterModifier(int paramaterCount) {
            if (paramaterCount <= 0)
                throw new ArgumentException(Environment.GetResourceString("Arg_ParmArraySize"));

            _byRef = new bool[paramaterCount];
        }

        public bool this[int index] {
            get {return _byRef[index]; }
            set {_byRef[index] = value;}
        }

    }
}


