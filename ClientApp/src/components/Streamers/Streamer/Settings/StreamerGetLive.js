import React, {useState, useEffect} from "react";
import {InputLabel, makeStyles, Snackbar, Switch, Typography} from "@material-ui/core";
import {Alert} from "@material-ui/lab";

const styles = makeStyles((theme) => ({
    root: {
        display: "flex"
    },
    grow: {
        flexGrow: 1
    },
    liveText: {
        alignSelf: "center"
    }
}));

export default function StreamerGetLive(streamer) {
    const [snackbarOpen, setSnackbarOpen] = useState(false);
    const [checked, setChecked] = useState(streamer.getLive);
    const classes = styles();
    
    function handleInputChange(e) {
        setChecked(!checked);
        SetGetLive(e.target.checked)
    }

    async function SetGetLive(status) {
        const response = await fetch('database/streamer?id=' + streamer.id,
            {
                method: 'put',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    getLive: status
                })
            });

        if (!response.ok) {
            alert("Backend server not responding! Please refresh the page.");
        }
    }
    
    function handleSnackbarClose(event, reason) {
        if (reason === "clickaway") {
            return;
        }
        
        setSnackbarOpen(false);
    }

    return (
        <div className={classes.root}>
            <Typography className={classes.liveText} variant={"body1"}>Get Live?</Typography>
            <div className={classes.grow}/>
            <Switch style={{right: 0}} checked={checked} onChange={handleInputChange} color="primary"/>
            <Snackbar open={snackbarOpen} autoHideDuration={6000} onClose={handleSnackbarClose}>
                <Alert onClose={handleSnackbarClose} severity="error">
                    Could not save settings! Please check your network configuration.
                </Alert>
            </Snackbar>
        </div>
    )
}