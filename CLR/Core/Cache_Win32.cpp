////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_USE_AVLTREE_FOR_METHODLOOKUP)

struct TreeDiagram
{
    CLR_RT_EventCache::LookupEntry* node;
    int                             depth;
    int                             depthOfLeftChildren;
    int                             depthOfRightChildren;
    size_t                          parent;
    size_t                          center;
    size_t                          leftChild;
    size_t                          rightChild;

    TreeDiagram()
    {
        node                 = NULL;
        depthOfLeftChildren  = 0;
        depthOfRightChildren = 0;
        depth                = 0;
        parent               = -1;
        center               = -1;
        leftChild            = -1;
        rightChild           = -1;
    }

    bool HasChildren()
    {
        return leftChild != -1 || rightChild != -1;
    }

    bool DrawHorizontalBar( size_t col )
    {
        if(leftChild  != -1 && col > leftChild  && col < center) return true;
        if(rightChild != -1 && col < rightChild && col > center) return true;

        return false;
    }
};

typedef std::vector< TreeDiagram > TreeDiagram_Vector;

static void BuildLevel( TreeDiagram_Vector& vec, CLR_RT_EventCache::LookupEntry* node, int& depthTot, int depth, TreeDiagram*& tdThis )
{
    if(node)
    {
        TreeDiagram  tdLocal;
        TreeDiagram* tdChild;
        int          depthLeft  = 0;
        int          depthRight = 0;

        tdLocal.node  = node;
        tdLocal.depth = depth;

        if(node->m_left ) depthLeft  = ++depthTot;
        if(node->m_right) depthRight = ++depthTot;

        //--//

        BuildLevel( vec, (CLR_RT_EventCache::LookupEntry*)node->m_left, depthTot, depthLeft, tdChild );
        if(tdChild)
        {
            tdLocal.leftChild           = tdChild->center;
            tdLocal.depthOfLeftChildren = max(tdChild->depthOfLeftChildren,tdChild->depthOfRightChildren) + 1;
        }

        tdLocal.center = vec.size(); vec.push_back( TreeDiagram() );

        BuildLevel( vec, (CLR_RT_EventCache::LookupEntry*)node->m_right, depthTot, depthRight, tdChild );
        if(tdChild)
        {
            tdLocal.rightChild           = tdChild->center;
            tdLocal.depthOfRightChildren = max(tdChild->depthOfLeftChildren,tdChild->depthOfRightChildren) + 1;
        }

        //--//

        if(tdLocal.leftChild  != -1) vec[ tdLocal.leftChild  ].parent = tdLocal.center;
        if(tdLocal.rightChild != -1) vec[ tdLocal.rightChild ].parent = tdLocal.center;

        //--//

        TreeDiagram& tdNew = vec[ tdLocal.center ];

        tdNew = tdLocal;

        tdThis = &tdNew;
    }
    else
    {
        tdThis = NULL;
    }
}

//--//

void CLR_RT_EventCache::VirtualMethodTable::DumpTree()
{
    TreeDiagram_Vector vec;
    TreeDiagram*       tdChild;
    size_t             tot;
    size_t             col;
    int                row;
    int                depth = 0;

    BuildLevel( vec, (LookupEntry*)m_tree.m_root, depth, 0, tdChild );

    tot = vec.size();

    if(tot)
    {
        std::string nameCLS;
        std::string nameMD;

        printf( "Tree:\n\n" );

        for(row=0; row<=depth; row++)
        {
            TreeDiagram tdActive;

            for(int j=0; j<4; j++)
            {
                for(col=0; col<tot; col++)
                {
                    TreeDiagram& td = vec[ col ];
                    LPCSTR       sz = " ";

                    if(td.depth == row)
                    {
                        switch(j)
                        {
                        case 0:
                            tdActive = td;
                            sz       = "*";

                            {
                                LookupEntry* other;
                                int          res;

                                other = (LookupEntry*)td.node->m_left;
                                if(other)
                                {
                                    res = td.node->m_payload.Compare( other->m_payload );

                                    if(res != 1) sz = "#";
                                }

                                other = (LookupEntry*)td.node->m_right;
                                if(other)
                                {
                                    res = td.node->m_payload.Compare( other->m_payload );

                                    if(res != -1) sz = "#";
                                }
                            }
                            break;

                        case 1: if(td.HasChildren()) sz = "|"; break;
                        case 2: if(td.HasChildren()) sz = "+"; break;
                        }
                    }
                    else if(td.parent != -1 && vec[ td.parent ].depth == row)
                    {
                        switch(j)
                        {
                        case 2: sz = td.parent < col ? "\\" : "/"; break;
                        case 3: sz = "|";                          break;
                        }
                    }
                    else if(tdActive.DrawHorizontalBar( col ))
                    {
                        switch(j)
                        {
                        case 2: sz = "-"; break;
                        }
                    }
                    else if(td.parent != -1 && vec[ td.parent ].depth < row && row < td.depth)
                    {
                        sz = "|";
                    }

                    printf( sz );
                }

                if(j == 0 && tdActive.node)
                {
                    Payload&         pl = tdActive.node->m_payload;
                    CLR_RT_StringMap map;

                    printf( " %08x %4d %4d [%08x %08x] ", (size_t)tdActive.node, tdActive.depthOfLeftChildren, tdActive.depthOfRightChildren, pl.m_mdVirtual.m_data, pl.m_cls.m_data );

                    CLR_RT_TypeDef_Instance   instCLS; instCLS.InitializeFromIndex( pl.m_cls       );
                    CLR_RT_MethodDef_Instance instMD ; instMD .InitializeFromIndex( pl.m_mdVirtual );

                    if(instCLS.InitializeFromIndex( pl.m_cls       ) &&
                       instMD .InitializeFromIndex( pl.m_mdVirtual )  )
                    {
                        instCLS.m_assm->BuildClassName ( instCLS.m_target, nameCLS, false );
                        instMD .m_assm->BuildMethodName( instMD .m_target, nameMD , map   );

                        printf( " {%4d %s %s}", instMD.Hits(), nameCLS.c_str(), nameMD.c_str() );
                    }
                }

                printf( "\n" );
            }
        }
    }
}

bool CLR_RT_EventCache::VirtualMethodTable::ConsistencyCheck()
{
    int depth;

    return ConsistencyCheck( (LookupEntry*)m_tree.m_root, depth );
}

bool CLR_RT_EventCache::VirtualMethodTable::ConsistencyCheck( LookupEntry* node, int& depth )
{
    if(node)
    {
        LookupEntry* other;
        int          res;
        int          depthLeft;
        int          depthRight;

        other = (LookupEntry*)node->m_left;
        if(other)
        {
            res = node->m_payload.Compare( other->m_payload );

            if(res != 1) return false;

            if(!ConsistencyCheck( other, depthLeft )) return false;
        }
        else
        {
            depthLeft = 0;
        }

        other = (LookupEntry*)node->m_right;
        if(other)
        {
            res = node->m_payload.Compare( other->m_payload );

            if(res != -1) return false;

            if(!ConsistencyCheck( other, depthRight )) return false;
        }
        else
        {
            depthRight = 0;
        }

        depth = max( depthLeft, depthRight ) + 1;
    }
    else
    {
        depth = 0;
    }

    return true;
}

#endif

