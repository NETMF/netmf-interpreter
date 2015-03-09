////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;

namespace Microsoft.SPOT.Wireless.IEEE_802_15_4.Network.Fragmentation
{
    internal class FragmentationMessageAssociationSet
    {
        private ArrayList _fragmentationMessagesArray;

        public FragmentationMessageAssociationSet()
        {
            _fragmentationMessagesArray = new ArrayList();
        }

        /// <summary>
        /// Get the active message associated to a remote node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public FragmentationMessage GetFragmentationMessage(UInt16 source, UInt16 destination)
        {
            lock (this)
            {
                int fragmentMessageArrayCount = _fragmentationMessagesArray.Count;
                for (int i = 0; i < fragmentMessageArrayCount; i++)
                {
                    FragmentationMessage msg = (FragmentationMessage)_fragmentationMessagesArray[i];
                    if ((msg.Source == source) && (msg.Destination == destination))
                    {
                        return msg;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Remove all associations and dispose all fragmentation messages.
        /// </summary>
        public void Dispose()
        {
            lock (this)
            {
                int fragmentMessageArrayCount = _fragmentationMessagesArray.Count;
                for (int i = 0; i < fragmentMessageArrayCount; i++)
                {
                    FragmentationMessage msg = (FragmentationMessage)_fragmentationMessagesArray[i];
                    msg.Dispose();
                }

                _fragmentationMessagesArray.Clear();
            }
        }

        /// <summary>
        /// Set the fragmentation message associated to a pair of transmitting nodes.
        /// </summary>
        internal void SetFragmentationMessage(UInt16 source, UInt16 destination, FragmentationMessage fragmentationMessage)
        {
            if ((source != fragmentationMessage.Source) || (destination != fragmentationMessage.Destination))
            {
                throw new ArgumentException("Inconsistent parameters.");
            }

            lock (this)
            {

                // first. check whether there is already an active message associated to this node.
                FragmentationMessage prevFragmentationMessage = GetFragmentationMessage(source, destination);
                if (prevFragmentationMessage != null)
                {
                    // there is already one active message associated with the node.
                    // remove this message first.
                    _fragmentationMessagesArray.Remove(prevFragmentationMessage);
                }

                _fragmentationMessagesArray.Add(fragmentationMessage);
                return;
            }

        }

        public void RemoveFragmentationMessage(FragmentationMessage fragmentationMessage)
        {
            lock (this)
            {

                _fragmentationMessagesArray.Remove(fragmentationMessage);

            }
        }

        public bool IsEmpty()
        {
            lock (this)
            {
                if (_fragmentationMessagesArray.Count == 0)
                    return true;
                else
                    return false;
            }
        }

    }
}


