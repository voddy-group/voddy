import React, {useState} from "react";
import {Box, Button, createMuiTheme, makeStyles, TextField, ThemeProvider, Typography} from "@material-ui/core";
import {green, orange, red} from "@material-ui/core/colors";
import YtDlpThreadCount from "./YtDlpThreadCount";

export default function YtDlpUpdate() {
    const [ytDlpStatus, setYtDlpStatus] = useState("Test installation.");

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

        </div>
    )
}