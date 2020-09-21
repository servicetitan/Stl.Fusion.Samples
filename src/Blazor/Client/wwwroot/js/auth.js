(function() {

let openAuthWindow = function (action, flowName) {
    let encode = encodeURIComponent;
    let redirectUrl = new URL("/close?flow=" + encode(flowName), document.baseURI).href;
    let url = "/" + action + "?returnUrl=" + encode(redirectUrl);
    return window.open(url, "_blank", "width=600,height=600;left=80;top=80");
}

window.Auth = {
    sessionId: "",
    getSessionId: function () {
        return this.sessionId;
    },
    signIn: function () {
        openAuthWindow("signin", "Sign-in");
    },
    signOut: function () {
        openAuthWindow("signout", "Sign-out");
    }
};

})();

