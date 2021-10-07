parser grammar MintDParser;

options { tokenVocab=MintDLexer; }

primitiveType
    : VOID
    | STRING
    | INT
    | BOOL
    | FLOAT
    | Identifier
    ;

type: primitiveType REF?;

literal
    : INTEGER_LITERAL
    | BACK_STRING+
    | DOUBLE_STRING+
    | FLOAT_LITERAL 
    | HEX_INTEGER_LITERAL
    | TRUE
    | FALSE
    ;

breakStatement: BREAK SEMI;

continueStatement: CONTINUE SEMI;

returnStatement: RETURN expression? SEMI;

block: LBRACE statementList? RBRACE;

functionSpecifier
    : SERVER
    | CLIENT
    ;

functionDeclaration: functionSpecifier type Identifier LPAR parameters? RPAR block;

parameters: parameter ( ',' parameter )*;
parameter: type Identifier;

statement
    : declarationStatement
    | embeddedStatement
    ;

embeddedStatement
    : block
    | nullStatement
    | expressionStatement
    | selectionStatement
    | iterationStatement
    | jumpStatement
    ;

declarationStatement
    : functionDeclaration
    | local_variable_declaration SEMI
    ;


nullStatement: SEMI;
expressionStatement: expression SEMI;
selectionStatement: ifStatement | switchStatement;

ifStatement
    : IF LPAR expression RPAR embeddedStatement elseStatement?;

elseStatement
    : ELSE embeddedStatement;

switchStatement: SWITCH LPAR expression RPAR LBRACE switchCases RBRACE;

switchCases: caseStatement (caseStatement)*;

caseStatement: caseLabel (statement)*;
caseLabel
    : CASE LPAR expression RPAR
    | DEFAULT
    ;

iterationStatement
    : whileStatement
    | forStatement
    | DO embeddedStatement WHILE LPAR expression RPAR
    ;

whileStatement: WHILE LPAR expression RPAR embeddedStatement;
forStatement: FOR LPAR forInitExp? SEMI forCondExp? SEMI forLoopExp? RPAR embeddedStatement;
forInitExp
    : expression
    | local_variable_declaration
    ;

forCondExp: expression;
forLoopExp: expression;
 
jumpStatement: breakStatement | continueStatement | returnStatement;

statementList: statement (statement)*;

local_variable_declaration: type variable_declarator ( COMMA variable_declarator )*;
variable_declarator: Identifier (ASSIGNMENT expression)?;

expression
    : assignmentExp
    | nonAssignmentExp
    ;
assignment_operator
	: ASSIGNMENT | ADD_ASSIGN | SUB_ASSIGN | MULT_ASSIGN | DIV_ASSIGN | MOD_ASSIGN | AND_ASSIGN | OR_ASSIGN
	;
assignmentExp: unaryExp assignment_operator expression;
nonAssignmentExp: conditionalOrExp;
conditionalOrExp: conditionalAndExp (LOGICAL_OR conditionalAndExp)*;
conditionalAndExp: equalityExp (LOGICAL_AND equalityExp)*;
equalityExp: relationalExp ((OP_EQ | OP_NE) relationalExp)*;
relationalExp: additiveExp ((LT | GT | LTE | GTE) additiveExp)*;
additiveExp
    : multiplicativeExp ((PLUS | MINUS) multiplicativeExp)*
    ;
multiplicativeExp: unaryExp ((TIMES | DIVIDE | MODULO) unaryExp)*;
unaryExp
    : postfixExp
    | preIncDecExp
    | unaryOperator unaryExp
    | typeCast unaryExp
    ;

typeCast: LPAR type RPAR;

unaryOperator
    : MINUS 
    | PLUS
    | BANG
    ;

preIncDecExp
    : INC unaryExp
    | DEC unaryExp
    ;
postfixExp
    : primaryExp 
    | postfixExp postfix;
postfix
    : methodInvocation 
    | memberAccess 
    | INC
    | DEC
    | localizeParams
    ;

methodInvocation: LPAR argList? RPAR methodExtern?;

methodExtern: EXTERN LPAR BACK_STRING RPAR;

argList: expression (COMMA expression)*;

memberAccess: DOT Identifier;

primaryExp
    : literal 
    | Identifier 
    | parenthExp 
    | localizeExp
    | DOLLAR LPAR expression RPAR
    ;

parenthExp: LPAR expression RPAR;
localizeExp: LSQBRACKET DOUBLE_STRING RSQBRACKET;
localizeParams: localizeParam (localizeParam)*;
localizeParam: OP_STREAM expression;