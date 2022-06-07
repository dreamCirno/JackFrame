using System;
using System.Collections.Generic;

namespace JackAST {

    public static class ASTFactory {

        public static IClassEditor CreateClassEditor() {
            return new ClassEditor();
        }

        public static IMethodEditor CreateMethodEditor() {
            return new MethodEditor();
        }

        public static IInterfaceEditor CreateInterfaceEditor() {
            return new InterfaceEditor();
        }

    }
    
}