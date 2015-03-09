////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

void CLR_RT_AVLTree::Initialize()
{
    NATIVE_PROFILE_CLR_CORE();
    m_root = NULL; // Entry*    m_root;
                   // OwnerInfo m_owner;
}

CLR_RT_AVLTree::Result CLR_RT_AVLTree::Insert( Entry* newDatum )
{
    NATIVE_PROFILE_CLR_CORE();
    return Insert( m_root, newDatum );
}

CLR_RT_AVLTree::Result CLR_RT_AVLTree::Remove( Entry* oldDatum )
{
    NATIVE_PROFILE_CLR_CORE();
    return Remove( m_root, oldDatum );
}

CLR_RT_AVLTree::Entry* CLR_RT_AVLTree::Find( Entry* srcDatum )
{
    NATIVE_PROFILE_CLR_CORE();
    Entry*      node  = m_root;
    ComparerFtn ftn   = m_owner.m_ftn_compare;
    void*       state = m_owner.m_state;

    while(node)
    {
        int cmp = ftn( state, node, srcDatum ); if(cmp == 0) break;

        if(cmp > 0)
        {
            node = node->m_left;
        }
        else
        {
            node = node->m_right;
        }
    }

    return node;
}

//////////////////////////////////////////////////

//
// Perform counterclockwise rotation.
//
void CLR_RT_AVLTree::RotateLeft( Entry*& n )
{
    NATIVE_PROFILE_CLR_CORE();
    //////////////////////////
    //                      //
    // A            A       //
    //  \            \      //
    //   B      =>    \     //
    //  / \            \    //
    //     C            C   //
    //    / \          / \  //
    //                B     //
    //               / \    //
    //                      //
    //////////////////////////
    Entry* top    = n;
    Entry* bottom = top->m_right;

    top   ->m_right = bottom->m_left;
    bottom->m_left  = top;

    n = bottom;
}

//
// Perform clockwise rotation.
//
void CLR_RT_AVLTree::RotateRight( Entry*& n )
{
    NATIVE_PROFILE_CLR_CORE();
    //////////////////////////
    //                      //
    //      A          A    //
    //     /          /     //
    //    B    =>    /      //
    //   / \        /       //
    //  C          C        //
    // / \        / \       //
    //               B      //
    //              / \     //
    //                      //
    //////////////////////////

    Entry* top    = n;
    Entry* bottom = top->m_left;

    top   ->m_left  = bottom->m_right;
    bottom->m_right = top;

    n = bottom;
}

//
// LeftGrown: helper function for avlinsert
//
// Parameters:
//
//   n           Reference to the pointer to a node. This node's left
//               subtree has just grown due to item insertion; its
//               "skew" flag needs adjustment, and the local tree
//               (the subtree of which this node is the root node) may
//               have become unbalanced.
//
// Return values:
//
//   RES_OK      The local tree could be rebalanced or was balanced
//               from the start. The parent activations of the avlinsert
//               activation that called this function may assume the
//               entire tree is valid.
//
//   RES_BALANCE The local tree was balanced, but has grown in height.
//               Do not assume the entire tree is valid.
//
CLR_RT_AVLTree::Result CLR_RT_AVLTree::LeftGrown( Entry*& n )
{
    NATIVE_PROFILE_CLR_CORE();
    Entry* left;
    Entry* right;

    switch(n->m_skew)
    {
    case SKEW_LEFT:
        left = n->m_left;

        if(left->m_skew == SKEW_LEFT)
        {
            n   ->m_skew = SKEW_NONE;
            left->m_skew = SKEW_NONE;

            RotateRight( n );
        }
        else
        {
            right = left->m_right;

            switch(right->m_skew)
            {
            case SKEW_LEFT:
                n   ->m_skew = SKEW_RIGHT;
                left->m_skew = SKEW_NONE;
                break;

            case SKEW_RIGHT:
                n   ->m_skew = SKEW_NONE;
                left->m_skew = SKEW_LEFT;
                break;

            default:
                n   ->m_skew = SKEW_NONE;
                left->m_skew = SKEW_NONE;
            }

            right->m_skew = SKEW_NONE;

            RotateLeft ( n->m_left );
            RotateRight( n         );
        }
        return RES_OK;

    case SKEW_RIGHT:
        n->m_skew = SKEW_NONE;
        return RES_OK;

    default:
        n->m_skew = SKEW_LEFT;
        return RES_BALANCE;
    }
}

///*
// *  avlrightgrown: helper function for avlinsert
// *
// *  See avlleftgrown for details.
// */
CLR_RT_AVLTree::Result CLR_RT_AVLTree::RightGrown( Entry*& n )
{
    NATIVE_PROFILE_CLR_CORE();
    Entry* left;
    Entry* right;

    switch(n->m_skew)
    {
    case SKEW_LEFT:
        n->m_skew = SKEW_NONE;
        return RES_OK;

    case SKEW_RIGHT:
        right = n->m_right;

        if(right->m_skew == SKEW_RIGHT)
        {
            n    ->m_skew = SKEW_NONE;
            right->m_skew = SKEW_NONE;

            RotateLeft( n );
        }
        else
        {
            left = right->m_left;

            switch(left->m_skew)
            {
            case SKEW_RIGHT:
                n    ->m_skew = SKEW_LEFT;
                right->m_skew = SKEW_NONE;
                break;

            case SKEW_LEFT:
                n    ->m_skew = SKEW_NONE;
                right->m_skew = SKEW_RIGHT;
                break;

            default:
                n    ->m_skew = SKEW_NONE;
                right->m_skew = SKEW_NONE;
            }

            left->m_skew = SKEW_NONE;

            RotateRight( n->m_right );
            RotateLeft ( n          );
        }
        return RES_OK;

    default:
        n->m_skew = SKEW_RIGHT;
        return RES_BALANCE;
    }
}

//
// LeftShrunk: helper function for Remove and FindLowest
//
// Parameters:
//
//   n           Reference to a pointer to a node. The node's left
//               subtree has just shrunk due to item removal; its
//               "skew" flag needs adjustment, and the local tree
//               (the subtree of which this node is the root node) may
//               have become unbalanced.
//
//  Return values:
//
//   RES_OK      The parent activation of the Remove activation
//               that called this function may assume the entire
//               tree is valid.
//
//   RES_BALANCE Do not assume the entire tree is valid.
//
CLR_RT_AVLTree::Result CLR_RT_AVLTree::LeftShrunk( Entry*& n )
{
    NATIVE_PROFILE_CLR_CORE();
    Entry* left;
    Entry* right;

    switch(n->m_skew)
    {
    case SKEW_LEFT:
        n->m_skew = SKEW_NONE;
        return RES_BALANCE;

    case SKEW_RIGHT:
        right = n->m_right;

        if(right->m_skew == SKEW_RIGHT)
        {
            n    ->m_skew = SKEW_NONE;
            right->m_skew = SKEW_NONE;

            RotateLeft( n );

            return RES_BALANCE;
        }
        else if(right->m_skew == SKEW_NONE)
        {
            n    ->m_skew = SKEW_RIGHT;
            right->m_skew = SKEW_LEFT;

            RotateLeft( n );

            return RES_OK;
        }
        else
        {
            left = right->m_left;

            switch(left->m_skew)
            {
            case SKEW_LEFT:
                n    ->m_skew = SKEW_NONE;
                right->m_skew = SKEW_RIGHT;
                break;

            case SKEW_RIGHT:
                n    ->m_skew = SKEW_LEFT;
                right->m_skew = SKEW_NONE;
                break;

            default:
                n    ->m_skew = SKEW_NONE;
                right->m_skew = SKEW_NONE;
            }

            left->m_skew = SKEW_NONE;

            RotateRight( n->m_right );
            RotateLeft ( n          );

            return RES_BALANCE;
        }

    default:
        n->m_skew = SKEW_RIGHT;
        return RES_OK;
    }
}

//
// RightShrunk: helper function for Remove and FindHighest
//
// See LeftShrunk for details.
//
CLR_RT_AVLTree::Result CLR_RT_AVLTree::RightShrunk( Entry*& n )
{
    NATIVE_PROFILE_CLR_CORE();
    Entry* left;
    Entry* right;

    switch(n->m_skew)
    {
    case SKEW_RIGHT:
        n->m_skew = SKEW_NONE;
        return RES_BALANCE;

    case SKEW_LEFT:
        left = n->m_left;

        if(left->m_skew == SKEW_LEFT)
        {
            n   ->m_skew = SKEW_NONE;
            left->m_skew = SKEW_NONE;

            RotateRight( n );

            return RES_BALANCE;
        }
        else if(left->m_skew == SKEW_NONE)
        {
            n   ->m_skew = SKEW_LEFT;
            left->m_skew = SKEW_RIGHT;

            RotateRight( n );

            return RES_OK;
        }
        else
        {
            right = left->m_right;

            switch(right->m_skew)
            {
            case SKEW_LEFT:
                n   ->m_skew = SKEW_RIGHT;
                left->m_skew = SKEW_NONE;
                break;

            case SKEW_RIGHT:
                n   ->m_skew = SKEW_NONE;
                left->m_skew = SKEW_LEFT;
                break;

            default:
                n   ->m_skew = SKEW_NONE;
                left->m_skew = SKEW_NONE;
            }

            right->m_skew = SKEW_NONE;

            RotateLeft ( n->m_left );
            RotateRight( n         );

            return RES_BALANCE;
        }

    default:
        n->m_skew = SKEW_LEFT;
        return RES_OK;
    }
}

//
// FindHighest: replace a node with a subtree's highest-ranking item.
//
// Parameters:
//
//   n           Reference to pointer to subtree.
//
//   target      Pointer to node to be replaced.
//
//   res         Pointer to variable used to tell the caller whether
//               further checks are necessary; analog to the return
//               values of LeftGrown and LeftShrunk (see there).
//
// Return values:
//
//   true        A node was found; the target node has been replaced.
//
//   false       The target node could not be replaced because
//               the subtree provided was empty.
//
//
bool CLR_RT_AVLTree::FindHighest( Entry*& n, Entry* target, Result& res )
{
    NATIVE_PROFILE_CLR_CORE();
    Entry* toFree;

    res = RES_BALANCE;

    if(!n)
    {
        return false;
    }

    if(n->m_right)
    {
        if(!FindHighest( n->m_right, target, res )) return false;

        if(res == RES_BALANCE)
        {
            res = RightShrunk( n );
        }

        return true;
    }

    m_owner.m_ftn_reassignNode( m_owner.m_state, n, target );

    toFree = n;
    n      = n->m_left;

    m_owner.m_ftn_freeNode( m_owner.m_state, toFree );

    return true;
}

//
// FindLowest: replace node with a subtree's lowest-ranking item.
//
// See FindHighest for the details.
//
bool CLR_RT_AVLTree::FindLowest( Entry*& n, Entry* target, Result& res )
{
    NATIVE_PROFILE_CLR_CORE();
    Entry* toFree;

    res = RES_BALANCE;

    if(!n)
    {
        return false;
    }

    if(n->m_left)
    {
        if(!FindLowest( n->m_left, target, res ))
        {
            return false;
        }

        if(res == RES_BALANCE)
        {
            res = LeftShrunk( n );
        }

        return true;
    }

    m_owner.m_ftn_reassignNode( m_owner.m_state, n, target );

    toFree = n;
    n      = n->m_right;

    m_owner.m_ftn_freeNode( m_owner.m_state, toFree );

    return true;
}

//
// Insert: insert a node into the AVL tree.
//
// Parameters:
//
//   n           Reference to a pointer to a node.
//
//   newDatum    Item to be inserted.
//
// Return values:
//
//   true        The item has been inserted. The excact value of
//               nonzero yields is of no concern to user code; when
//               avlinsert recursively calls itself, the number
//               returned tells the parent activation if the AVL tree
//               may have become unbalanced; specifically:
//
//     RES_OK        None of the subtrees of the node that n points to
//                   has grown, the AVL tree is valid.
//
//     RES_BALANCE   One of the subtrees of the node that n points to
//                   has grown, the node's "skew" flag needs adjustment,
//                   and the AVL tree may have become unbalanced.
//
//   false       The datum provided could not be inserted, either due
//               to key collision (the tree already contains another
//               item with which the same key is associated), or
//               due to insufficient memory.
//
CLR_RT_AVLTree::Result CLR_RT_AVLTree::Insert( Entry*& n, Entry* newDatum )
{
    NATIVE_PROFILE_CLR_CORE();
    Result res;
    Entry* node = n;

    if(!node)
    {
        node = m_owner.m_ftn_newNode( m_owner.m_state, newDatum ); if(!node) return RES_ERROR;

        node->m_left  = NULL;
        node->m_right = NULL;
        node->m_skew  = SKEW_NONE;

        n = node;

        return RES_BALANCE;
    }

    int cmp = m_owner.m_ftn_compare( m_owner.m_state, node, newDatum );

    if(cmp > 0)
    {
        res = Insert( node->m_left, newDatum );

        return res == RES_BALANCE ? LeftGrown( n ) : res;
    }

    if(cmp < 0)
    {
        res = Insert( node->m_right, newDatum );

        return res == RES_BALANCE ? RightGrown( n ) : res;
    }

    return RES_DUPLICATE;
}

//
// Remove: remove an item from the tree.
//
// Parameters:
//
//   n           Reference of a pointer to a node.
//
//   key         key of item to be removed.
//
// Return values:
//
//   true        The item has been removed. The exact value of
//               nonzero yields if of no concern to user code; when
//               avlremove recursively calls itself, the number
//               returned tells the parent activation if the AVL tree
//               may have become unbalanced; specifically:
//
//     RES_OK        None of the subtrees of the node that n points to
//                   has shrunk, the AVL tree is valid.
//
//     RES_BALANCE   One of the subtrees of the node that n points to
//                   has shrunk, the node's "skew" flag needs adjustment,
//                   and the AVL tree may have become unbalanced.
//
//  false        The tree does not contain an item yielding the
//               key value provided by the caller.
//
CLR_RT_AVLTree::Result CLR_RT_AVLTree::Remove( Entry*& n, Entry* oldDatum )
{
    NATIVE_PROFILE_CLR_CORE();
    Result res;

    if(!n)
    {
        return RES_NOTFOUND;
    }

    int cmp = m_owner.m_ftn_compare( m_owner.m_state, n, oldDatum );

    if(cmp > 0)
    {
        res = Remove( n->m_left, oldDatum );

        return res == RES_BALANCE ? LeftShrunk( n ) : res;
    }

    if(cmp < 0)
    {
        res = Remove( n->m_right, oldDatum );

        return res == RES_BALANCE ? RightShrunk( n ) : res;
    }

    if(n->m_left)
    {
        if(FindHighest( n->m_left, n, res ))
        {
            return res == RES_BALANCE ? LeftShrunk( n ) : res;
        }
    }

    if(n->m_right)
    {
        if(FindLowest( n->m_right, n, res ))
        {
            return res == RES_BALANCE ? RightShrunk( n ) : res;
        }
    }

    m_owner.m_ftn_freeNode( m_owner.m_state, n );

    n = NULL;

    return RES_BALANCE;
}
