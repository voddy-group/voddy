import React, {useState, useEffect} from "react";
import {InputLabel, makeStyles, Snackbar, Switch, Typography} from "@material-ui/core";

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

export default function SearchAddSettingsGetLive(GetLive) {
    const [checked, setChecked] = useState(false);
    const classes = styles();

    function handleInputChange(e) {
        setChecked(!checked);
        GetLive.setGetLive(checked);
    }

    return (
        <div className={classes.root}>
            <Typography className={classes.liveText} variant={"body1"}>Get Live?</Typography>
            <div className={classes.grow}/>
            <Switch style={{right: 0}} checked={checked} onChange={handleInputChange} color="primary"/>
        </div>
    )
}