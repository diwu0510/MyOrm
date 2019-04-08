using System.Collections.Generic;
using System.Linq.Expressions;

namespace HZC.MyOrm.Expressions
{
    public class ObjectMemberVisitor : ExpressionVisitor
    {
        private readonly List<string> _propertyList;

        public ObjectMemberVisitor()
        {
            _propertyList = new List<string>();
        }

        public List<string> GetPropertyList()
        {
            return _propertyList;
        }

        public void Clear()
        {
            _propertyList.Clear();
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
            {
                _propertyList.Add(node.Member.Name);
            }
            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            foreach (var arg in node.Arguments)
            {
                if (arg.NodeType == ExpressionType.MemberAccess)
                {
                    var member = (MemberExpression) arg;
                    if (member.Expression != null && member.Expression.NodeType == ExpressionType.Parameter)
                    {
                        _propertyList.Add(member.Member.Name);
                    }
                }
            }
            return node;
        }
    }
}
