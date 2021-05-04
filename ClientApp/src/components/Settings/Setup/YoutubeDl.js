import React, {useState} from "react";
import {Box, Button, createMuiTheme, makeStyles, TextField, ThemeProvider, Typography} from "@material-ui/core";
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
    },
    headerStyling: {
        marginTop: 10
    }
}));

export default function YoutubeDl() {
    const [youtubeDlStatus, setYoutubedlStatus] = useState("Test installation.");
    const [hide, setHide] = useState(true);
    const [youtubeDlPath, setYoutubeDlPath] = useState("");
    const [updateButtonDisabled, setUpdateButtonDisabled] = useState(false);
    const [updateButtonText, setUpdateButtonText] = useState("Force Download/Update Youtube-dl")
    const [buttonStyle, setButtonStyle] = useState(null);
    const classes = styles();

    function handleChangeYoutubeDlPath(e) {
        setYoutubeDlPath(e.target.value);
    }

    async function TestYoutubeDl() {
        setYoutubedlStatus("Testing...");
        setButtonStyle(classes.orangeButton);
        const response = await fetch('youtubeDl/test' +
            '?path=' + youtubeDlPath, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            },
        });

        const returnedData = await response.json();

        if (returnedData.error == null) {
            setYoutubedlStatus("Working!");
            setButtonStyle(classes.greenButton);
            setHide(true);
        } else {
            setYoutubedlStatus("Broken! Error: " + returnedData.error);
            setButtonStyle(classes.redButton);
            setHide(false);
        }
    }

    async function ForceUpdateYoutubeDl() {
        setUpdateButtonText("Downloading...");
        setUpdateButtonDisabled(true);
        const response = await fetch('youtubeDl/update',
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
            <Typography variant="h4" className={classes.headerStyling}>Youtube-dl</Typography>
            <legend />
            <Typography variant="body1">Youtube-dl is required to download streams. Keeping Youtube-dl up to date is very important in keeping
                voddy running smoothly.</Typography>
            <Typography>Test Youtube-dl installation:</Typography>
            <div className={classes.inputDiv}>
                <Button className={buttonStyle} variant="contained" color="primary"
                        onClick={TestYoutubeDl}>{youtubeDlStatus}</Button>
            </div>
            <div className={classes.inputDiv}>
                <Button variant="contained" color="primary" disabled={updateButtonDisabled}
                        onClick={ForceUpdateYoutubeDl}>{updateButtonText}</Button>
            </div>
            <div className={hide ? 'hidden' : ''}>
                <p>Make sure Youtube-dl is intalled, and test again.</p>
                <p>If you are sure you have Youtube-dl installed, entire the path here (e.g. "/usr/bin/youtube-dl" or
                    "C:/youtube-dl/youtube-dl.exe":</p>
                <TextField error={youtubeDlPath.length === 0} className={classes.input} variant="outlined"
                           onChange={handleChangeYoutubeDlPath} />
                <p>Run the test again to verify the new path.</p>
            </div>
        </div>
    )
}