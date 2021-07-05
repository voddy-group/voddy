import React, {useEffect, useState} from "react";
import {Button, CircularProgress, makeStyles, Snackbar, Switch, TextField, Typography} from "@material-ui/core";
import {MergeTypeRounded} from "@material-ui/icons";
import {Alert} from "@material-ui/lab";
import {green, red} from "@material-ui/core/colors";

const styles = makeStyles((theme) => ({
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
    }
}));

export default function Path() {
    const [path, setPath] = useState(null);
    const [loading, setLoading] = useState(false);
    //const [runningMigration, setRunningMigration] = useState("No currently running migrations.");
    const [openSnackbar, setOpenSnackbar] = useState(false);
    const [alertSeverity, setAlertSeverity] = useState(null);
    const [alertText, setAlertText] = useState(null);
    const [buttonStyle, setButtonStyle] = useState(null);
    const classes = styles();

    useEffect(() => {
        GetCurrentPath();
    }, [])

    function handlePathChange(event) {
        setPath(event.target.value);
    }

    /*async function CheckForRunningMigrations() {
        const request = await fetch('path/migrationRunning',
            {
                method: 'get',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        
        if (request.ok) {
            var response = await request.json();
            if (response.running) {
                setDisabled(true);
                setRunningMigration("Migration running! Please wait for it to finish before changing the path again.");
            }
        }
    }*/

    async function GetCurrentPath() {
        const request = await fetch('path',
            {
                method: 'get',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        if (request.ok) {
            var response = await request.json();
            setPath(response.path);
        }
    }

    async function handleButtonClick() {
        setLoading(true);
        const request = await fetch('path',
            {
                method: 'put',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    newPath: path
                })
            });

        if (request.ok) {
            setLoading(false);
            setButtonStyle(classes.greenButton);
            setAlertSeverity("success");
            setAlertText("Path saved successfully! New path is " + path);
            setOpenSnackbar(true);
        } else {
            setLoading(false);
            setButtonStyle(classes.redButton);
            setAlertSeverity("error");
            setAlertText("Path could not be saved; please check if the path is valid and this application has write permissions.")
            setOpenSnackbar(true);
        }
    }
    
    function handleSnackbarClose(event, reason) {
        if (reason === "clickaway") {
            return;
        }
        
        setOpenSnackbar(false);
    }

    //todo give <a> below a link, explain symlink as alternative

    return (
        <div>
            <Typography variant={"h3"} color={"primary"}>Storage</Typography>
            <legend />
            <Typography variant={"body1"}>By default, voddy files will be stored in your operating systems default
                application storage area (AppData for Windows, /var/lib for Linux-based). Due to the large amount of
                space required, this will probably not suit most users. Here, you can change the storage location of all
                files generated by this program to a more suitable location.</Typography>
            <Typography variant={"body1"}>It is advised that this is changed before you begin downloading streams,
                during the setup phase of the application, but it can be done at any time.</Typography>
            <Typography variant={"body1"}>You will need to move the folders and files in your original storage path
                to the new path that you enter below.</Typography>
            <Typography variant={"body1"}>Please enter the new storage location below:</Typography>
            <TextField onChange={handlePathChange} id="standard-basic" label="Path" value={path} InputLabelProps={{shrink: true}} onKeyPress={(bttn) => {
                if (bttn.key === 'Enter')
                    handleButtonClick();
            }} />
            <div className={classes.inputDiv}>
                <Button variant={"contained"} color={"primary"} className={buttonStyle} onClick={handleButtonClick}>
                {loading ?
                    <CircularProgress/>
                    :
                    <Typography variant={"body1"}>Save</Typography>
                }</Button>
            </div>
            <Snackbar open={openSnackbar} autoHideDuration={6000} onClose={handleSnackbarClose}>
                <Alert severity={alertSeverity}>{alertText}</Alert>
            </Snackbar>
        </div>
    )
}