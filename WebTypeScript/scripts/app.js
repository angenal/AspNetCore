var Tsl;
(function (Tsl) {
    Tsl[Tsl["Red"] = 0] = "Red";
    Tsl[Tsl["Green"] = 1] = "Green";
    Tsl[Tsl["Blue"] = 2] = "Blue";
})(Tsl || (Tsl = {}));
function sayHello() {
    var ts2;
    var _a = ts2 = [
        document.getElementById("compiler").value,
        $("#framework").val(),
        1
    ], _b = _a[0], compiler = _b === void 0 ? '' : _b, _c = _a[1], framework = _c === void 0 ? '' : _c;
    return "Hello from " + compiler + " and " + framework + ", is " + Tsl[ts2.pop()] + "'s inputs!";
}
function infiniteLoop() {
    while (true) { }
}
//# sourceMappingURL=app.js.map