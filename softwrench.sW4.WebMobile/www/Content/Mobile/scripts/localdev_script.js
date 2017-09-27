//dev data that should be used only at development mode --> using localhost tcottier/password
const localhostDevData = {
    //set to true to allow starting the app at the login screen
    showlogin: false,
    //    username: "fs112894",
    username: "tcottier",
    password: "password",
    //    password: "$@$Pass2",
    serverurl: {
        ripple: "http://localhost:8080/sw4",
        device: "http://localhost:8080/sw4"
    },
    debuglogs: ["init"]


};


//dev data that should be used only at development mode (Aaaron´s user) User Master password $@$Pass2
//FS VPN data
const fsVpnLocalData =  {

    //set to true to allow starting the app at the login screen
    showlogin: false,
    username: "fs112894",
    password: "$@$Pass2",
    serverurl: {
        ripple: "http://10.1.17.151/softwrench/",
    },
    debuglogs: ["init"]


};

//dev data pointing to dev --> using dev.softwrench.net tcottier/password
//FS VPN data
const devLocalData = {
    //set to true to allow starting the app at the login screen
    showlogin: false,
    //    username: "fs112894",
    username: "tcottier",
    password: "password",
    //    password: "$@$Pass2",
    serverurl: {
        ripple: "http://dev.softwrench.net/firstsolar",
        device: "http://dev.softwrench.net/firstsolar"
    },
    debuglogs: ["init"]
};


//dev data pointing to dev --> using dev.softwrench.net tcottier/password
//FS VPN data
const qaLocalData = {
    //set to true to allow starting the app at the login screen
    showlogin: false,
    //    username: "fs112894",
    username: "tcottier",
    password: "password",
    //    password: "$@$Pass2",
    serverurl: {
        ripple: "http://qa.softwrench.net/firstsolar",
        device: "http://qa.softwrench.net/firstsolar"
    },
    debuglogs: ["init"]
};



const fsVpnProdLocalData = {
    //set to true to allow starting the app at the login screen
    showlogin: false,
    username: "fs112894",
    password: "$@$Pass2",
    serverurl: {
        ripple: "http://10.2.8.65/softwrench/",
    },
    debuglogs: ["init"]


};

window.localdevdata = localhostDevData;

