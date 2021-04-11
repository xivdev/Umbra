using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbra.Util
{
    public class GenericTree
    {
        public class TreeNode
        {
            public string Fragment { get; set; }
            public string FullName { get; set; }
            public List< TreeNode > Children { get; } = new();
        }

        public List< TreeNode > Nodes { get; private set; } = new();

        public void FolderSort()
        {
            // ?????????
            Nodes = Nodes.OrderByDescending( n => n.Children.Any() ).ToList();
        }

        public void AddNode( string path )
        {
            // figure out the basic shit first
            var fragments = path.Split( '/' );
            var currentFragment = fragments[ ^1 ];

            var parent = GetOrCreateParentFolderNode( fragments );

            // check if we have a child with the current node already
            if( parent != null && parent.Any( n => n.Fragment.Equals( currentFragment, StringComparison.InvariantCultureIgnoreCase ) ) )
            {
                // already have it, don't care
                return;
            }

            // add the new node now
            var newNode = new TreeNode
            {
                Fragment = currentFragment,
                FullName = BuildFullPathForFragments( fragments, fragments.Length ),
            };

            if( parent == null )
            {
                Nodes.Add( newNode );
            }
            else
            {
                parent.Add( newNode );
            }
        }

        private TreeNode? FindApplicableNode( List< TreeNode > collection, string fragment )
        {
            foreach( var node in collection )
            {
                if( node.Fragment.Equals( fragment, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    return node;
                }
            }

            return null;
        }

        private List< TreeNode > GetOrCreateParentFolderNode( string[] fragments )
        {
            List< TreeNode > head = Nodes;

            for( var i = 0; i < fragments.Length - 1; i++ )
            {
                var current = fragments[ i ];

                var node = FindApplicableNode( head, current );
                if( node == null )
                {
                    // create new node
                    var newNode = new TreeNode
                    {
                        Fragment = current,
                        FullName = BuildFullPathForFragments( fragments, i ),
                        // IconKind = PackIconFontAwesomeKind.FolderSolid
                    };

                    head.Add( newNode );
                    head = newNode.Children;
                }
                else
                {
                    // we found some shit, fuck yea
                    head = node.Children;
                }
            }

            // at this point we should have the correct collection, minus the last fragment (which is the file itself)
            return head;
        }

        private string BuildFullPathForFragments( string[] fragments, int pos )
        {
            var frags = fragments.Take( pos + 1 );
            return string.Join( '/', frags );
        }
    }
}