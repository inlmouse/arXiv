using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace arXiv
{
    class TreeViewOperator
    {
        public static void setChildNodeCheckedState(TreeNode currNode, bool state) //将currNode的所有子节点checkbox状态设置为state
        {
            TreeNodeCollection nodes = currNode.Nodes;
            if (nodes.Count > 0)
                foreach (TreeNode tn in nodes)
                {
                    tn.Checked = state;
                    setChildNodeCheckedState(tn, state);//递归的目的是不过多少层子节点都设置为state状态
                }
        }
        public static void setParentNodeCheckedState(TreeNode currNode, bool state) //将currNode的父节点checkbox设置为state
        {
            TreeNode parentNode = currNode.Parent;

            parentNode.Checked = state;
            if (currNode.Parent.Parent != null)
            {
                setParentNodeCheckedState(currNode.Parent, state);//一直递归到topnode(祖宗辈节点）
            }
        }

        public static void getstring(TreeNode currNode)//从最左上角那个父节点开始遍历
        {
            if (currNode.Checked)
            {
                //string QX += currNode.Tag.ToString();//若被选中，则获取tag中的东东
            }
            TreeNodeCollection nodes = currNode.Nodes;//子节点集合
            if (nodes.Count > 0)
            {
                getstring(nodes[0]);//若有子节点，则递归
            }
            if (currNode.NextNode != null)
            {
                getstring(currNode.NextNode);//若存在同级的下一个节点，则继续递归
            }
        }
    }
}
