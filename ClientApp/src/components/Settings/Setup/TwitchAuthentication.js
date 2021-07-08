import React, {useEffect, useState} from "react";
import {
    Button,
    CircularProgress,
    createMuiTheme,
    makeStyles,
    MuiThemeProvider,
    TextField,
    ThemeProvider,
    Typography
} from "@material-ui/core";
import {green, orange, red} from "@material-ui/core/colors";

const styles = makeStyles((theme) => ({
    input: {
        width: "400px"
    },
    inputDiv: {
        paddingTop: "10px",
        paddingBottom: "10px"
    },
    greenButton: {
        backgroundColor: green[500],
        '&:hover': {
            backgroundColor: green[700]
        }
    },
    redButton: {
        backgroundColor: red[500],
        '&:hover': {
            backgroundColor: red[700]
        }
    },
    orangeButton: {
        backgroundColor: orange[500],
        '&:hover': {
            backgroundColor: orange[700]
        }
    }
}));

export default function TwitchAuthentication() {
    const [clientId, setClientId] = useState("");
    const [clientSecret, setClientSecret] = useState("");
    const [hideAuth, setHideAuth] = useState(true);
    const [hideAuthTokenRequest, setHideAuthTokenRequest] = useState(true);
    const [authUrl, setAuthUrl] = useState("");
    const [tokenVerefication, setTokenVerification] = useState("Getting Token...");
    const [checkAuthentication, setCheckAuthentication] = useState("Check authentication");
    const [hideProgress, setHideProgress] = useState(true);
    const [authButtonDisabled, setAuthButtonDisabled] = useState(true);
    const [buttonStyle, setButtonStyle] = useState(null);
    const classes = styles();

    let authWindow;
    
    useEffect(() => {
        fetchExistingClientIdSecret();
    }, [])

    function handleChangeClientId(e) {
        setClientId(e.target.value);
    }

    function handleChangeClientSecret(e) {
        setClientSecret(e.target.value);
    }

    function backendTestCredentials() {
        beginAuthProcess();
    }

    async function fetchExistingClientIdSecret() {
        const response = await fetch('auth/credentials', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });
        
        const returnedData = await response.json();
        
        if (response.ok) {
            setClientId(returnedData.clientId);
            setClientSecret(returnedData.clientSecret);
        }
    }
    
    async function beginAuthProcess() {
        const sendData = {
            clientId: clientId,
            clientSecret: clientSecret
        }
        const response = await fetch('auth/twitchAuth', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(sendData)
        });
        const returnedData = await response.json();

        if (response.ok) {
            setHideAuth(false);
            setAuthUrl(returnedData.url);
            setAuthButtonDisabled(false);
        }
    }

    function openAuthUrl() {
        authWindow = window.open(authUrl, 'mywindow', 'menubar=1,resizable=1,width=500,height=500');
        var id = setInterval(() => closeAuthUrl(id), 500);
        setHideProgress(false);
    }

    function closeAuthUrl(id) {
        try {
            if (authWindow.location.href.indexOf("auth/twitchAuth/redirect") < 0) {
                console.log("safe");
            } else {
                clearInterval(id);
                authWindow.close();
                getToken();
            }
        } catch (e) {
            // nasty way of doing it, but it works. could not find an alternative
        }
    }

    async function getToken() {
        setHideAuthTokenRequest(false);

        const response = await fetch('auth/twitchAuth/token', {
            Method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        const returnedData = await response.json();

        if (response.ok) {
            setTokenVerification("Success! You are now authenticated with twitch.");
        } else {
            setTokenVerification("Failed! Error: " + returnedData.error);
        }
        setHideProgress(true);
    }

    async function checkAuthenticationDetails() {
        setCheckAuthentication("Checking...");
        setButtonStyle(classes.orangeButton);

        var body = {
            "url": "https://api.twitch.tv/helix/streams",
        }

        const response = await fetch("twitchApi", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(body)
        });

        if (response.ok) {
            setCheckAuthentication("Success! Your current session is valid.")
            setButtonStyle(classes.greenButton);
        } else {
            setCheckAuthentication("Failure! Your current session is invalid, please re-auth.")
            setButtonStyle(classes.redButton);
        }
    }

    return (
        <div>
            <Typography variant="h4">Twitch Authentication</Typography>
            <legend />
            <Typography>You must sign in to twitch to allow voddy to use the API.</Typography>
            <Typography>Read the setup page to get started on creating an app in twitch, and retrieving the required details.</Typography>
            <div>
                <TextField error={clientId.length === 0} className={classes.input} label="Client ID" variant="outlined"
                           onChange={handleChangeClientId} value={clientId}/>
            </div>
            <div className={classes.inputDiv}>
                <TextField error={clientSecret.length === 0} className={classes.input} label="Client Secret" variant="outlined"
                           onChange={handleChangeClientSecret}
                           value={clientSecret}/>
            </div>
            <div className={classes.inputDiv}>
                <Button variant="contained" color="primary" disabled={clientId.length === 0 || clientSecret.length === 0} type="submit" onClick={backendTestCredentials}>Generate Auth Link</Button>
            </div>
            <div className={classes.inputDiv}>
                <Button variant="contained" color="primary" disabled={authButtonDisabled} onClick={openAuthUrl}>Authenticate
                    with twitch</Button>
            </div>
            <CircularProgress className={hideProgress ? 'hidden': ''} />
            <p className={hideAuthTokenRequest ? 'hidden' : ''}>{tokenVerefication}</p>
            <div className={classes.inputDiv}>
                    <Button className={buttonStyle} variant="contained" color="primary"
                            onClick={checkAuthenticationDetails}>{checkAuthentication}</Button>
            </div>
        </div>
    )
}