var currentlyOpenSidebar = "";
var sbRefresh = "";

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
function showElement(id) {
        var e = document.getElementById(id);

        e.style.display = 'block';
      
    }
function hideElement(id) {
        var e = document.getElementById(id);

        e.style.display = 'none';
    }
function toggle_visibility(id) {
        var e = document.getElementById(id);
        if (e.style.display == 'block')
            e.style.display = 'none';
        else
            e.style.display = 'block';
    }
function clearContent(id) {
        var e = document.getElementById(id);
            e.innerHTML = '';
    }
function EliminateElement(id) {
        var e = document.getElementById(id);
            e.outerHTML = '';
    }
function selectElementContents(el) {
        var body = document.body, range, sel;
        if (document.createRange && window.getSelection) {
            range = document.createRange();
            sel = window.getSelection();
            sel.removeAllRanges();
            try {
                range.selectNodeContents(el);
                sel.addRange(range);
            } catch (e) {
                range.selectNode(el);
                sel.addRange(range);
            }
            document.execCommand("copy");

        } else if (body.createTextRange) {
            range = body.createTextRange();
            range.moveToElementText(el);
            range.select();
            range.execCommand("Copy");
        }
        //alert('copied');
    }
function AjaxLoad(url, is_animate,afterAction) {
        is_animate = is_animate || "1";
        afterAction = afterAction || "";
        showElement('MainContentContainer');
        if(is_animate=="1"){
        showElement('ContentLoader');
        }

        if(currentlyOpenSidebar!=""){
            controller.close(currentlyOpenSidebar);
           AdjustMainView('right','CLOSE');
        }

        var xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200) {
                
                
                document.getElementById("MainContentDiv").innerHTML = this.responseText;
                if(is_animate=="1"){
                hideElement('ContentLoader');
                      }
            if(afterAction=="ROUNDMONITOR"){
                StartRoundMonitor();
            }
                      
            }  console.log(this.responseText);
        };

        xmlhttp.open("GET", url, true);
        xmlhttp.send();
    }
function Notify(msg, msg_type){
      msg_type = msg_type || 1;
        var bgColors = [
    "linear-gradient(to right, #00b09b, #96c93d)",
    "linear-gradient(to right, #ff5f6d, #ffc371)"
],
    i = 0;

// Options for the toast
var options = {
    text: msg,
    duration: 2500,
    callback: function () {
        this.remove();
        Toastify.reposition();
    },
    close: true,
    backgroundColor: "linear-gradient(to right, #00b09b, #96c93d)"
};

// Initializing the toast
var myToast = Toastify(options);

// Toast after delay
/*setTimeout(function () {
    myToast.showToast();
}, 3000);

setTimeout(function () {
    Toastify({
        text: "Highly customizable",
        gravity: "bottom",
        positionLeft: true,
        close: true,
        backgroundColor: "linear-gradient(to right, #ff5f6d, #ffc371)"
    }).showToast();
}, 2000);
*/

/*
Toastify({
    text: "Pure JavaScript Toasts",
    gravity: "bottom",
    positionLeft: false,
    backgroundColor: "#0f3443"
}).showToast();

// Displaying toast on manual action `Try`
document.getElementById('new-toast').addEventListener('click', function () {
    Toastify({
        text: "This is a toast",
        duration: 3000,
        backgroundColor: bgColors[i]
    }).showToast();
    i = i ? 0 : 1;
});

*/

Toastify({
    text: msg,
    duration: 4500,
    destination: 'https://github.com/apvarun/toastify-js',
    newWindow: true,
    gravity: "top",
    positionLeft: true
}).showToast();


    }
function GoSlider(sliding_element,is_autoClose,theDelay,keepContent,justClose) {
    is_autoClose = is_autoClose || 1;
    theDelay = theDelay || 4000;
    keepContent = keepContent || 1;
    justClose = justClose || 0;

    if(justClose!=0)
    {
        SlideOut(sliding_element,keepContent);
    }
    else
    {
        showElement(sliding_element);
        SlideIn(sliding_element);

        if(is_autoClose==1)
        {
            setTimeout(function () { SlideOut(sliding_element,keepContent); }, theDelay);
        }    
    }
    
    }
function isHidden(el){
    return (el.offsetParent === null)
    }
function reOpenPanel(btn_id) {
        var btn = document.getElementById(btn_id);
        btn.click();
        setTimeout(btn.click(), 500);
        console.log(btn_id);
        
}
function Accordionize(elem){
    var acc = document.getElementById(elem);
    acc.classList.toggle("active");
    var panel = acc.nextElementSibling;
    if (panel.style.maxHeight){
      panel.style.maxHeight = null;
    } else {
      panel.style.maxHeight = panel.scrollHeight + "px";
    }
    console.log("ACCING: " + elem);
}
function Accordionize2(elem){
    var acc = document.getElementById(elem);
    acc.onclick = function() {
    this.classList.toggle("active");
    var panel = this.nextElementSibling;
    if (panel.style.maxHeight){
      panel.style.maxHeight = null;
    } else {
      panel.style.maxHeight = panel.scrollHeight + "px";
    } 
  }
}
function IsAccordion(elem)    {
    var acc = document.getElementById(elem);
    var panel = acc.nextElementSibling;
    if(panel.style.maxHeight!=null && panel.style.maxHeight!=0){
        console.log('t');
        return true;
    }
    else{
        console.log('f');
        return false;

    }
    }
function reOpenAccordion(accElem){
        
            Accordionize(accElem);
            setTimeout(function () { Accordionize(accElem); }, 500);    
        

    }
function reOpenSideBar(sbElem){
    controller.close(sbElem);
    
    currentlyOpenSidebar = "";
        setTimeout(function () { controller.open(sbElem); currentlyOpenSidebar = sbElem;}, 500);

}
function sbload(sideBar, url, loader_element, form_name, open_after_load){
    loader_element = loader_element || sideBar + "Loader";
    form_name = form_name || "";
    open_after_load = open_after_load || false;

    var content_container = sideBar + "Content";

    console.log(sideBar);
    console.log(content_container);
    if (document.getElementById(content_container)) {
        document.getElementById(content_container).innerHTML = "";

    }
    
    if (!open_after_load) {
        if (currentlyOpenSidebar == sideBar) {
            reOpenSideBar(sideBar);
        }
        else {
            controller.open(sideBar);
        }
        currentlyOpenSidebar = sideBar;
    }


    if(document.getElementById(loader_element)){
        showElement(loader_element);    
    }

    var formData = new FormData();
    if(form_name!=""){
        if(document.getElementById(form_name)){
            var form = document.getElementById(form_name);
            formData = new FormData(form);
            console.log("form aqq: " + form)
        }
    }
        var xmlhttp = new XMLHttpRequest();
        xmlhttp.onreadystatechange = function () {
            if (this.readyState == 4 && this.status == 200) {
                var theRespone = this.responseText;
                console.log(theRespone);
                if (theRespone.startsWith("!$!")) {
                    console.log("AAAAAA");
                    let errMsgElement = "ErrorMsgDiv";
                    if (document.getElementById(errMsgElement)) {
                        document.getElementById(errMsgElement).innerHTML = theRespone.replace("!$!", "");
                    }
                    else {
                        Notify(theRespone.replace("!$!", ""), 3);
                    }
                    

                }
                else {
                    console.log("EEEEEEEE");
                    console.log("content_container: " + content_container);
                    document.getElementById(content_container).innerHTML = this.responseText;
                    if (open_after_load) {
                        console.log("BBBBBBB");
                        if (currentlyOpenSidebar == sideBar) {
                            reOpenSideBar(sideBar);
                            console.log("CCCCCCCCCCCCCC");
                        }
                        else {
                            console.log("DDDDDDDDDDDD");
                            controller.open(sideBar);
                        }
                        currentlyOpenSidebar = sideBar;
                    }
                }
                if(document.getElementById(loader_element)){
                    hideElement(loader_element);    
                   
                }
                //console.log(this.responseText);
           // location.hash = "#Top" + sideBar + "Mark";

        }
    };

    
    if (form_name != "") {
        xmlhttp.open("POST", url, true);
        xmlhttp.setRequestHeader("X-Requested-With", "XMLHttpRequest");
        xmlhttp.send(formData);
    } else {
        xmlhttp.open("GET", url, true);
        xmlhttp.setRequestHeader("X-Requested-With", "XMLHttpRequest");
        xmlhttp.send();
    }
    

}

    function FixAccordion(elem){
    var acc = document.getElementById(elem);
    var panel = acc.nextElementSibling;
    setTimeout(function () { panel.style.maxHeight = panel.scrollHeight + "px"; }, 500);

}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
