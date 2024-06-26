﻿using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using Dynamo.Core;
using Dynamo.Engine;
using Dynamo.Library;
using Newtonsoft.Json;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Graph.Nodes.ZeroTouch
{
    /// <summary>
    ///     DesignScript var-arg function node. All functions from DesignScript share the
    ///     same function node but internally have different procedure.
    /// </summary>
    [NodeName("Function Node w/ VarArgs"), NodeDescription("FunctionNodeDescription", typeof(Properties.Resources)),
    IsVisibleInDynamoLibrary(false), IsMetaNode]
    [AlsoKnownAs("Dynamo.Nodes.DSVarArgFunction")]
    public class DSVarArgFunction : DSFunctionBase
    {
        /// <summary>
        /// The function name with required parameters.
        /// </summary>
        public string FunctionSignature
        {
            get
            {
                return Controller.Definition.MangledName;
            }
        }

        /// <summary>
        /// It indicates which of the three types of function calls this node represents, 
        /// a call to an external graph, a call to a function with a vararg argument, 
        /// or a standard function.
        /// </summary>
        public string FunctionType
        {
            get
            {
                return "VariableArgument";
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DSVarArgFunction"/> class.
        /// </summary>
        /// <param name="descriptor">Function descritor.</param>
        public DSVarArgFunction(FunctionDescriptor descriptor)
            : base(new ZeroTouchVarArgNodeController<FunctionDescriptor>(descriptor))
        {
            VarInputController = new ZeroTouchVarInputController(this);
            defaultNumInputs = descriptor.Parameters.Count();
        }

        /// <summary>
        /// The NodeType property provides a name which maps to the 
        /// server type for the node. This property should only be
        /// used for serialization. 
        /// </summary>
        public override string NodeType
         {
             get
             {
                 return "FunctionNode";
             }
        }

        /// <summary>
        /// Returns the default number of inputs for the node
        /// </summary>
        private readonly int defaultNumInputs;

        [JsonIgnore]
        internal int DefaultNumInputs { get { return defaultNumInputs; } }

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context);
            VarInputController.SerializeCore(element, context);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context);
            VarInputController.DeserializeCore(nodeElement, context);
        }

        internal override bool HandleModelEventCore(string eventName, int value, UndoRedoRecorder recorder)
        {
            return VarInputController.HandleModelEventCore(eventName, value, recorder)
                || base.HandleModelEventCore(eventName, value, recorder);
        }

        /// <summary>
        ///     Custom VariableInput controller for DSVarArgFunctions.
        /// </summary>
        [JsonIgnore]
        public VariableInputNodeController VarInputController { get; private set; }

        #region VarInput Controller
        private sealed class ZeroTouchVarInputController : VariableInputNodeController
        {
            private readonly ZeroTouchNodeController<FunctionDescriptor> nodeController;

            public ZeroTouchVarInputController(DSFunctionBase model)
                : base(model)
            {
                nodeController = model.Controller;
            }
            /// <summary>
            /// This method is to get the index of the adding Input when we click +
            /// nodeController.Definition.Parameters.Count() will return
            /// the number of inputs the node got by default. For example, String.Join
            /// got separator+string0. when we click +, base.GetInputIndexFromModel() return 2,
            /// (nodeController.Definition.Parameters.Count() -1) return 1. Then the new port will
            /// be string1
            /// </summary>
            /// <returns></returns>
            public override int GetInputIndexFromModel()
            {
                return base.GetInputIndexFromModel() - (nodeController.Definition.Parameters.Count() -1);
            }

            protected override string GetInputName(int index)
            {
                return nodeController.Definition.Parameters.Last().Name.TrimEnd('s') + index;
            }

            protected override string GetInputTooltip(int index)
            {
                var type = nodeController.Definition.Parameters.Last().Type;
                return type.ToShortString();
            }
        }
        #endregion

    }

    /// <summary>
    ///     Controller that extends Zero Touch synchronization with VarArg function compilation.
    /// </summary>
    public class ZeroTouchVarArgNodeController<T> : ZeroTouchNodeController<T>
        where T : FunctionDescriptor
    {
        /// <summary>
        ///     Initializes a new instance of
        /// the <see cref="ZeroTouchVarArgNodeController{T}"/> class with FunctionDescriptor.
        /// </summary>
        /// <param name="zeroTouchDef">FunctionDescriptor describing the function
        /// that this controller will call.</param>
        public ZeroTouchVarArgNodeController(T zeroTouchDef) : base(zeroTouchDef) { }

        protected override void InitializeFunctionParameters(NodeModel model, IEnumerable<TypedParameter> parameters)
        {
            var typedParameters = parameters as IList<TypedParameter> ?? parameters.ToList();
            base.InitializeFunctionParameters(model, typedParameters.Take(typedParameters.Count() - 1));
            if (parameters.Any())
            {
                var arg = parameters.Last();
                var argName = arg.Name.Remove(arg.Name.Length - 1) + "0";
                model.InPorts.Add(new PortModel(PortType.Input, model, new PortData(argName, arg.Description, arg.DefaultValue)));
            }
        }

        protected override void BuildOutputAst(NodeModel model, List<AssociativeNode> inputAstNodes, List<AssociativeNode> resultAst)
        {
            // All inputs are provided, then we should pack all inputs that
            // belong to variable input parameter into a single array.
            if (!model.IsPartiallyApplied)
            {
                var paramCount = Definition.Parameters.Count();

                // Suppose a fucntion foo() with var args, its signature is:
                //
                //    foo(x1, x2, ..., xn, params y)
                //
                // so paramCount == n + 1 here, and suppose inputs are
                //
                //        i1, i2, ...., in, y1, y2, ..., ym
                //
                // Here we pack all var arguments in an array {y1, y2, ..., ym}
                // (skipping the first n == paramCount - 1 inputs)
                var argPack = AstFactory.BuildExprList(inputAstNodes.Skip(paramCount - 1).ToList());
                inputAstNodes = inputAstNodes.Take(paramCount - 1).ToList();
                inputAstNodes.Add(argPack);
            }

            base.BuildOutputAst(model, inputAstNodes, resultAst);
        }
    }
}