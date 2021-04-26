const NODE_COUNT_WITHOUT_ERROR_MESSAGE = 11;


$(document).ready(() => {
    document.getElementById("FL-filename-search-form").addEventListener("submit", async (e) => {
        e.preventDefault();
        findFile();
    })
})
function addValidationErrors(eleId, name, accesslevelId, isNameTaken = false) {
    let validationErrorMessage = document.getElementById(eleId);
    console.log(eleId, "isNameTaken:", isNameTaken,`   ${!name ? '<li>Folder name field is required</li>' : ''}
            ${!accesslevelId ? '<li>Access level field is required</li>' : ''}
            ${isNameTaken ? `<li>${name} already exist in folder</li>` : ''}   
        `)
    validationErrorMessage.innerHTML =
        `   ${!name ? '<li>Folder name field is required</li>' : ''}
            ${!accesslevelId ? '<li>Access level field is required</li>' : ''}
            ${isNameTaken ? `<li>${name} already exist in folder</li>` : ''}   
        `
    console.log("validation added: ", validationErrorMessage)
}
async function getPartialView(url, id) {
    url =  `${url}?id=${id}` 
    
    var alertMessageBox = document.getElementById('alertMessageBox');
    let resp = await fetch(url)
    
    if (resp.status === 403) {
        alertMessageBox.innerHTML = "Action Restricted: Action cannot be executed on folder";
        alertMessageBox.className = 'showMessage task-failure';
        setTimeout(() => { alertMessageBox.className = ''; return; }, 3000);
        return;
    }
    else if (resp.status===500){
        alertMessageBox.innerHTML = "Server error: Action failed";
        alertMessageBox.className = 'showMessage task-failure';
        setTimeout(() => { alertMessageBox.className = ''; return; }, 3000)
        return;
    }

    await showModal(resp)
    await monitorUserActivityOnModal(url);
}

async function showModal(resp) {
    var modalBody = document.getElementById("modalBody")
    let textResp = await resp.text();
    modalBody.innerHTML = textResp;
    $("#modal").modal("show");
}

async function ConfirmMove(folderToMoveInto,newParentFolderId) {
    let copiedItem = JSON.parse(localStorage.getItem("copiedItem"));
    var moveFolderAlert = document.getElementById('alertMessageBox');
    console.log(copiedItem.time, new Date(Date.now() - new Date(copiedItem.time)).getHours())
    if (!copiedItem) {
        moveFolderAlert.innerHTML = "Error: No Item was copied";
        moveFolderAlert.className = 'showMessage task-failure';
        setTimeout(() => { moveFolderAlert.className = ''; return; }, 2000)
    }
    else {
        url = `/Folder/GetConfirmItemMovePartialView?itemName=${copiedItem.name}&currentFolder=${folderToMoveInto}&newParentFolderId=${newParentFolderId}`;
        console.log(url)
        let resp = await fetch(url);
        if (resp.status === 200) {
            await showModal(resp);
            document.getElementById("confirmItemMove-form").addEventListener("submit", async (e) => {
                e.preventDefault
                console.log("newParentFolderId:", newParentFolderId)
                await CtrlV(parseInt(newParentFolderId));
            })
        }
        else  {
            moveFolderAlert.innerHTML = "Server error: Action failed";
            moveFolderAlert.className = 'showMessage task-failure';
            setTimeout(() => { alertMessageBox.className = ''; return; }, 3000)
            return;
        }
    }
   
}

async function monitorUserActivityOnModal(url) {
    if (url.includes("folders/add")) {
        document.getElementById("createFolder").addEventListener("submit", async (e) => {
            e.preventDefault();
            await createFolder(url);
        })
    }
    else if (url.includes("/Folder/GetEditFolderPartialView")) {
        document.getElementById("editFolder").addEventListener("submit", async (e) => {
            e.preventDefault();
            await editFolder();
        })
    }
    else {
        document.getElementById("delFolder-no").addEventListener("click", async (e) => {
            closeModal();
        })
        document.getElementById("delFolder-form").addEventListener("submit", async (e) => {
            e.preventDefault();
            await deleteFolder();
        })

    }
}

async function deleteFolder() {
    let verificationToken = document.getElementsByName("__RequestVerificationToken")[0].value;
    let folderId = document.getElementById("delFolder-id").value;
    let parentId = document.getElementById("delFolder-parentId").value;
    
    let resp = await fetch("/folders/delete", {
        method: "POST",
        headers: {
            'Content-Type': 'application/json',
            __RequestVerificationToken: verificationToken,
        },
        body: JSON.stringify({
            id: parseInt(folderId),
            ParentId: parseInt(parentId)
        })
    })
    if (resp.status === 200) {
        location.reload();
    }
    else if (resp.status === 400) {
        let div = document.getElementById("delfolder-con");
        div.innerHTML =
            `<div class='validation-summary-errors'>
                <ul>
                    <li>Delete Action cannot be performed on this folder</li>
                </ul>
            </div>`
        document.getElementsByClassName("modal-title")[0].innerHTML = "";
        
    }
    return;
}

function closeModal() {
    $("#modal").modal("hide");
}

async function editFolder() {
    let verificationToken = document.getElementsByName("__RequestVerificationToken")[0].value
    let accesslevelId = document.getElementById("EF-accessLevel").value;
    let name = document.getElementById("EF-name").value;
    let parentId = document.getElementById("EF-parentId").value;
    let folderId = document.getElementById("EF-id").value;

    if (!name || !accesslevelId) {
        console.log("got heer")
        addValidationErrors("EF-validation-errors", name, accesslevelId)
    }
    else {
        let res = await postData("/Folder/Edit", name, accesslevelId, parentId, verificationToken, id = folderId)
        if (res.status == 400) {
            console.log("got here")
            addValidationErrors("EF-validation-errors", name,accesslevelId,true)
        }
        else if (res.status==200) {
            location.reload();
        }
       
    }
    }

async function createFolder(url) {
    let verificationToken = document.getElementsByName("__RequestVerificationToken")[0].value
    let parentId = document.getElementById("CF-parentId").value;
    let accesslevelId = document.getElementById("CF-accessLevel").value;
    let name = document.getElementById("CF-name").value;
    
    if (!name || !accesslevelId) {
        addValidationErrors("CF-validation-errors", name, accesslevelId)
    }
    else {
        let resp = await postData(url, name, accesslevelId, parentId, verificationToken, 0)
        console.log(resp.status === 400)
        console.log(resp)
        if (resp.status === 400) {
            addValidationErrors("CF-validation-errors", name, accesslevelId,true)
        }
        else {
            location.reload()
        } 
        
    }
   
}

async function postData(url, name, accesslevelId, parentId, token, id = 0) {
    let resp = await fetch(url, {
        method: "POST",
        headers: {
            'Content-Type': 'application/json',
            __RequestVerificationToken: token,
        },
        body: JSON.stringify({
            Name: name,
            AccessLevelId: parseInt(accesslevelId),
            ParentId: parseInt(parentId),
            Id: id
        })
    })
    return resp
}

function CtrlX(itemId, itemName,itemType) {
    var copyFolderAlert = document.getElementById('alertMessageBox');
    copyFolderAlert.innerHTML = `${itemName} ${itemType=="folder"? "folder": "file"} copied`;
    copyFolderAlert.className = 'showMessage task-success';
    localStorage.setItem("copiedItem", JSON.stringify({ "id": itemId, "itemType": itemType, name: itemName, time: Date.now() }))
    setTimeout(() => { copyFolderAlert.className = ''; return; }, 2000)
}

async function CtrlV(newParentFolderId ) {
    let copiedItem = JSON.parse(localStorage.getItem("copiedItem"));
    let verificationToken = document.getElementsByName("__RequestVerificationToken")[0].value;
    var moveFolderAlert = document.getElementById('alertMessageBox');
    console.log("newParentFolderId:", newParentFolderId)
    resp = await fetch("/folders/move", {
        method: "POST",
        headers: {
            __RequestVerificationToken: verificationToken,
            'content-type': "application/json"
        },
        body: JSON.stringify({
            id: copiedItem.id,
            fileType: copiedItem.itemType,
            newParentFolderId: newParentFolderId
        })
    })
 
    if (resp.status === 200) {
        localStorage.removeItem("copiedItem")
        moveFolderAlert.innerHTML = `Item moved successfully`;
        moveFolderAlert.className ="showMessage task-success"
        location.reload();
    }
    else {
        moveFolderAlert.innerHTML =(
            resp.status === 403 ? `Item with name already exist in Folder` :
                resp.status === 405 ? "Warning: Cannot move a folder into itself" : "Server Error: Action Failed.");
        moveFolderAlert.className = "showMessage task-failure";
        setTimeout(() => { alertMessageBox.className = ''; return; }, 3000)
    }
}

async function VerifyAccessToken(event) {
    event.preventDefault();
    document.getElementById("verifyaccesstoken-btn").disabled = true;
    document.getElementById("confirming-code").style.display = "block";
    let verificationToken = document.getElementsByName("__RequestVerificationToken")[0].value;
    let input = document.getElementById('EAC-code')
    var accessCode =input.value;
    var returnUrl = input.getAttribute("data-returnUrl");
    resp = await fetch("/Folder/VerifyAccessCode", {
        method: "POST",
        headers: {
            __RequestVerificationToken: verificationToken,
            'content-type': "application/json"
        },
        body: JSON.stringify({
            accessCode: accessCode,
            
        })
    })
    let validationMessageDiv = document.getElementById("EAC-form").children[1];
    let respJson = await resp.json();
    document.getElementById("confirming-code").style.display = "none";
    if (respJson.Status === 200) {
        validationMessageDiv.innerHTML = "";
        Swal.fire({
            position: 'top-end',
            icon: 'success',
            title: 'Access Granted!!!',
            showConfirmButton: false,
            timer: 2000
        });
        
        location.href = returnUrl;
        return;
    }
    else {
        document.getElementById("verifyaccesstoken-btn").disabled = false;
        Swal.fire({
            position: 'top-end',
            icon: 'error',
            title: `${respJson.Message ? respJson.Message: "internal server error"}`,
            showConfirmButton: false,
            timer: 2000
        });
        validationMessageDiv.innerHTML = (
            `<ul>
                <li>${respJson.Message}</li>
            </ul>`)
        
    }

}

async function findFile() {
    document.getElementById("FL-filename-search-form").addEventListener("submit", async (e) => {
        e.preventDefault();
    })
    let filename = document.getElementById("FL-filename-search-input").value;
    let folderId = document.getElementById("FL-filename-search-input").getAttribute("data-folderId");
    if (filename) {
        let resp = await fetch(`/folder/getfile?filename=${filename}&folderId=${folderId}`)
        console.log(resp.status)
        if (resp.status === 200) {
            let textresp = await resp.text();
            document.getElementById("FL-files").innerHTML = textresp;
        }
    }
    else {
        let resp = await fetch(`/folder/getfile?filename=${filename}&folderId=${folderId}&returnall=true`)
        if (resp.status === 200) {
            let textresp = await resp.text();
            document.getElementById("FL-files").innerHTML = textresp;
        }
    }
    
}

const getSubfolders = async (event) => {
    console.log(event.target.id, event.target.className)
    if (event.target.className === "far fa-folder") {
        let folderId = event.target.id.split("-")[1];
        let resp = await fetch(`/folder/getsubfolders?folderId=${folderId}`)
        if (resp.status === 200) {
            let respJson = await resp.json()
            let ul=getUlElement(respJson)
            document.getElementById(event.target.id).parentNode.parentNode.parentNode.appendChild(ul);
        }   
    }
    else if (event.target.className === "far fa-folder-open") {
        let currentSubfolder = document.getElementById(event.target.id).parentNode.parentNode.parentNode;
        let currentSubfolderChildNodes = currentSubfolder.childNodes
        console.log(currentSubfolderChildNodes)
        if (currentSubfolderChildNodes.length==4) {
            currentSubfolder.removeChild(currentSubfolderChildNodes[3])
        }

    }

    event.target.className === "far fa-folder" ?
        document.getElementById(`${event.target.id}`).className = "far fa-folder-open" :
        document.getElementById(`${event.target.id}`).className = "far fa-folder" 
    

}

const getUlElement =(data)=>{
    let ul = document.createElement("ul");
    ul.className = "file-menu subfolder-child";
    ul.id = "FL-folders";
    data.map(x => {
        ul.innerHTML += `
        <li>  
            <div style="display:block">
                <div style="display:flex;justify-content:space-between;align-items: center;" class="folderlist-folder">
                    <i class="far fa-folder" id="subfolder-${x.Id}" onclick="getSubfolders(event)"></i>
                    <a href=${`/folders/${x.Id}`} >${x.Name} </a>
                    <a href="" class="dropdown-link" data-toggle="dropdown" style="padding:0;width: 3em;margin:0; display:flex;justify-content:center">
                        <i class="fa fa-ellipsis-v"></i>
                    </a>
                    <div class="dropdown-menu dropdown-menu-right">

                        <a href="#" class="dropdown-item" onclick="getPartialView('/Folder/GetEditFolderPartialView',${x.id})">
                            Edit Folder
                        </a>
                        <a href="#" class="dropdown-item" onclick="getPartialView('/Folder/GetDeleteFolderPartialView',${x.id})">
                            Delete Folder
                        </a>
                        <a href="#" class="dropdown-item"
                            onclick="CtrlX(${x.Id}, ${x.Name}', 'folder')"> Cut Folder</a>
                </div>
            </div>

            </div>
        </li>
    `;
        return x
    })
    return ul;
}


const resendOTP = async (event) => {
    
    let resp = await fetch("folders/resendotp")
    document.getElementById("confirming-code").style.display = "block";
    event.target.disabled = true;
    if (resp.status == 200) {
        Swal.fire({
            position: 'top-end',
            icon: 'success',
            title: 'OTP sent check your email.',
            showConfirmButton: false,
            timer: 2000
        });
        event.target.innerHTML = "Resend OTP in <span id='otpCounter'>60</span> secs";
        event.target.disabled = true;
        document.getElementById("confirming-code").style.display = "none";
        startResendOTPCountdown();
    }
    else {
        event.target.disabled = false;
        Swal.fire({
            position: 'top-end',
            icon: 'info',
            title: 'Failed to send accesscode',
            showConfirmButton: false,
            timer: 2000
        });
        document.getElementById("confirming-code").style.display = "none";
    }
}

const startResendOTPCountdown = () => {
    let secondsLeft = parseInt(document.getElementById("otpCounter").textContent);
    if (secondsLeft > 0) {
        document.getElementById("otpCounter").parentNode.disabled = true;
        document.getElementById("otpCounter").textContent = --secondsLeft;
        setTimeout(() => { startResendOTPCountdown() }, 1000)
    }
    else {
        document.getElementById("otpCounter").parentNode.disabled = false;
        document.getElementById("otpCounter").parentNode.textContent = "Resend OTP"
    }
    
    

}

