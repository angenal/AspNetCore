/// <reference path="../node_modules/@types/jquery/index.d.ts" />

//@author angenal
//@description app test scripts
//@ignore 第一行是引用第三方模块declare,所有declare申明都是已实现的功能(要引入相关js,并且变量名要存在)

/** 申明 全局变量 */
declare var window: Window;
/** 申明 只读变量 */
//declare const $: JQueryStatic;
declare interface JQuery { /** 扩展 */asTooltip(): any; }
declare namespace JQuery {
    let num: number;
    const version: string;
    /**
     * 设置版本
     * @param v 版本号
     */
    function setVersion(v: string): void;
    interface fn { /** 扩展 */asTooltip(): any; }
}
/** 申明 当前域变量 */
declare let N1: number;
/** 申明 全局函数(newDiv必须有相关js引入) */
declare function newDiv(): HTMLDivElement;
declare function getWidget(n: number): Widget;
/** 申明 覆盖/新增 */
declare function getWidget(s: string): Widget[];
declare function getWidgetIndexOf(a: Widget): number | Widget;

/** 定义新的类型 */
type Widget = HTMLElement | (() => HTMLElement) | number;
type Tsa = [string, string, number];
enum Tsl { Red = 0, Green, Blue }


/** 输出Hello */
function sayHello() {
    let ts2: Tsa;
    const [compiler = '', framework = ''] = ts2 = [
        (document.getElementById("compiler") as HTMLInputElement).value,
        $("#framework").val() as string,
        1
    ];
    return `Hello from ${compiler} and ${framework}, is ${Tsl[<number>ts2.pop()]}'s inputs!`;
}
/** 无限循环 */
function infiniteLoop() {
    while (true) { }
}

