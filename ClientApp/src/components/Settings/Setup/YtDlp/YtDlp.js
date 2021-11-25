import React, {useEffect, useState} from "react";
import {Box, Button, createMuiTheme, makeStyles, TextField, ThemeProvider, Typography} from "@material-ui/core";
import {green, orange, red} from "@material-ui/core/colors";
import YtDlpThreadCount from "./YtDlpThreadCount";

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
    },
    headerStyling: {
        marginTop: 10
    }
}));

export default function YtDlp() {
    const [ytDlpStatus, setYtDlpStatus] = useState("Test installation.");
    const [hide, setHide] = useState(true);
    const [ytDlpPath, setYtDlpPath] = useState("");
    const [updateButtonDisabled, setUpdateButtonDisabled] = useState(false);
    const [updateButtonText, setUpdateButtonText] = useState("Force Download/Update yt-dlp")
    const [buttonStyle, setButtonStyle] = useState(null);
    const [globalSettings, setGlobalSettings] = useState([]);
    const [maxThreads, setMaxThreads] = useState(0);
    const [currentThreads, setCurrentThreads] = useState(0);
    const [updateAvailable, setUpdateAvailable] = useState(false);
    const classes = styles();

    useEffect(async function() {
        getGlobalSettings();
    }, [])
    
    async function getGlobalSettings() {
        const request = await fetch('setup/globalSettings', {
            method: 'get',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (request.ok) {
            var response = await request.json();
            for (var x = 0; x < response.length; x++) {
                if (response[x].key === "workerCount") {
                    setMaxThreads(JSON.parse(response[x].value).AvailableThreads);
                }

                if (response[x].key === "ytDlpThreadCount") {
                    setCurrentThreads(response[x].value);
                }
                
                if (response[x].key === "yt-dlpUpdate" && response[x].value === "True") {
                    setUpdateAvailable(true);
                    setUpdateButtonText("Update available!");
                }
            }
        }
    }
    
    function handleChangeYtDlpPath(e) {
        setYtDlpPath(e.target.value);
    }

    async function TestYtDlp() {
        setYtDlpStatus("Testing...");
        setButtonStyle(classes.orangeButton);
        const response = await fetch('ytDlp/test' +
            '?path=' + ytDlpPath, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
        });

        if (response.ok) {
            setYtDlpStatus("Working!");
            setButtonStyle(classes.greenButton);
            setHide(true);
        } else {
            const returnedData = await response.json();
            setYtDlpStatus("Broken! Error: " + returnedData.error);
            setButtonStyle(classes.redButton);
            setHide(false);
        }
    }

    async function ForceUpdateYtDlp() {
        setUpdateButtonText("Downloading...");
        setUpdateButtonDisabled(true);
        const response = await fetch('ytDlp/update',
            {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        if (response.ok) {
            setUpdateButtonText("Updated!")
            setUpdateButtonDisabled(false);
        }
    }

    return (
        <div>
            <Typography variant="h4" className={classes.headerStyling}>yt-dlp</Typography>
            <legend/>
            <Typography variant="body1">yt-dlp is required to download streams. Keeping yt-dlp up to date is very
                important in keeping
                voddy running smoothly.</Typography>
            <Typography>Test yt-dlp installation:</Typography>
            <div className={classes.inputDiv}>
                <Button className={buttonStyle} variant="contained" color="primary"
                        onClick={TestYtDlp}>{ytDlpStatus}</Button>
            </div>
            <div className={classes.inputDiv}>
                <Button variant="contained" color="primary" disabled={updateButtonDisabled}
                        onClick={ForceUpdateYtDlp}>{updateButtonText}</Button>
            </div>
            <div className={hide ? 'hidden' : ''}>
                <p>Make sure yt-dlp is intalled, and test again.</p>
                <p>If you are sure you have yt-dlp installed, entire the path here (e.g. "/usr/bin/yt-dlp" or
                    "C:/yt-dlp/yt-dlp.exe":</p>
                <TextField error={ytDlpPath.length === 0} className={classes.input} variant="outlined"
                           onChange={handleChangeYtDlpPath}/>
                <p>Run the test again to verify the new path.</p>
            </div>
            <YtDlpThreadCount maxThreads={maxThreads} currentThreads={currentThreads} />
        </div>
    )
}