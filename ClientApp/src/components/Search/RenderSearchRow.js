import React, {useState} from "react";
import "../../assets/styles/StreamSearch.css";
import {
    Button, ButtonBase,
    CircularProgress, Dialog, DialogContent, DialogTitle,
    Grid,
    GridListTile,
    GridListTileBar,
    IconButton,
    makeStyles, MenuItem, Select, Snackbar,
    SvgIcon,
    Typography
} from "@material-ui/core";
import SearchAddSettingsQuality from "./SearchAddSettings/SearchAddSettingsQuality";
import SearchAddSettingsGetLive from "./SearchAddSettings/SearchAddSettingsGetLive";
import SearchAddSettings from "./SearchAddSettings/SearchAddSettings";
import {Link} from "react-router-dom";

const styles = makeStyles((theme) => ({
    GridListTile: {
        marginTop: 2,
        marginBottom: 2,
        maxWidth: "100%",
        backgroundColor: "lightblue",
        borderRadius: 10,
        position: "relative"
    },
    topTileBar: {
        backgroundColor: "unset"
    },
    liveText: {
        alignSelf: "center"
    },
    select: {
        width: 150
    },
    grow: {
        flexGrow: 1,
        minWidth: 50
    },
    button: {
        height: 216,
        width: "100%",
        position: "absolute"
    }
}));

export default function RenderSearchRow(searchedData) {
    const [alreadyAdded, setAlreadyAdded] = useState(false);
    const [streamer] = useState(searchedData.searchedData);
    const [settingsOpen, setSettingsOpen] = useState(false);
    const [quality, setQuality] = useState({"resolution": 0, "fps": 0});
    const [getLive, setGetLive] = useState(false);
    const classes = styles();


    if (streamer.alreadyAdded && !alreadyAdded) {
        setAlreadyAdded(true);
    }

    function handleButtonClicked() {

        changeStreamStatus();
    }

    async function changeStreamStatus() {
        var body = {
            "streamerId": streamer.streamerId,
            "displayName": streamer.displayName,
            "username": streamer.username,
            "thumbnailUrl": streamer.thumbnailLocation
        }

        console.log(JSON.stringify(body));

        const response = await fetch('database/streamer' +
            '?isNew=true',
            {
                method: 'post',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(body)
            });

        if (response.ok) {
            added();
        }
    }

    function added() {
        // when click user gets relocated to local streamer page
    }

    function handleClickSettings() {
        if (!streamer.alreadyAdded)
            setSettingsOpen(!settingsOpen);
    }

    return (
        <Grid container spacing={2} className={classes.GridListTile} key={searchedData.key}>
            <Button component={streamer.alreadyAdded ? Link : "button"} to={streamer.alreadyAdded ? "/streamer/" + streamer.id : null}
                        onClick={handleClickSettings}
                        className={classes.button}/>
            <Grid item>
                <img alt="thumbnail" style={{width: 200, height: 200}} src={streamer.thumbnailLocation}/>
            </Grid>
            <Grid item xs={12} sm container style={{pointerEvents: "none"}}>
                <Grid item xs container direction={"column"} style={{padding: 8}}>
                    <Typography variant={"h3"}>{streamer.displayName}</Typography>
                    <Typography style={{display: "inline-block"}} variant={"body1"}>
                        <SvgIcon>
                            <path fill="white"
                                  d="M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17M12,4.5C7,4.5 2.73,7.61 1,12C2.73,16.39 7,19.5 12,19.5C17,19.5 21.27,16.39 23,12C21.27,7.61 17,4.5 12,4.5Z"/>
                        </SvgIcon>{streamer.viewCount ? streamer.viewCount.toLocaleString() : streamer.viewCount} Views</Typography>
                    <Typography style={{display: "inline-block"}} variant={"body1"}>
                        <SvgIcon
                            style={{display: "inline-block"}}>
                            <path fill="white"
                                  d="M13,9H11V7H13M13,17H11V11H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z"/>
                        </SvgIcon>
                        {streamer.description}
                    </Typography>
                </Grid>
            </Grid>
            <SearchAddSettings streamer={streamer} setQuality={setQuality} setGetLive={setGetLive}
                               settingsOpen={settingsOpen} setSettingsOpen={setSettingsOpen}/>
        </Grid>
    )
}
