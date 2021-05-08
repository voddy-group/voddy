import React, {useState, useEffect} from "react";
import StreamersRows from "./StreamersRows";
import {
    AppBar, //Grid,
    GridList,
    GridListTile,
    GridListTileBar,
    isWidthUp,
    makeStyles, Slider,
    Toolbar,
    Typography
} from "@material-ui/core";

import Streamer from "./Streamer/Streamer";

const styles = makeStyles((theme) => ({
    root: {
        display: "flex",
        flexWrap: "wrap",
        overflow: "hidden",
        width: "100%"
    },
    iconSizeSlider: {
        width: 100
    }
}));

export default function Streamers() {
    const [streamers, setStreamers] = useState([]);
    const [iconSize, setIconSize] = useState(getIconSizeCookie);
    const [defaultSliderValue] = useState(getDefaultSliderValue);
    const classes = styles();

    useEffect(() => {
        getStreamers();
    }, [])
    
    function getIconSizeCookie() {
        var iconSizeCookie;
        iconSizeCookie = parseInt(getCookieValue("iconSize"));
        return iconSizeCookie != null ? iconSizeCookie : 300;
    }
    
    function getDefaultSliderValue() {
        var defaultSliderValueCookie;
        defaultSliderValueCookie = parseInt(getCookieValue("defaultSliderValue"));
        return defaultSliderValueCookie != null ? defaultSliderValueCookie : 2;
    }
    
    function getCookieValue(key) {
        if (document.cookie != null) {
            var split = document.cookie.split(";")
            if (split != null) {
                var find = split.find(row => row.replaceAll(" ", "").startsWith(key + "="))
                if (find != null) {
                    return find.split("=")[1];
                }
            }
        }
    }

    async function getStreamers() {
        const request = await fetch('database/streamers',
            {
                Method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        var response = await request.json();
        setStreamers(response.data);
    }
    
    function changeIconSize(event, value) {
        switch (value) {
            case 1:
                setIconSize(250);
                document.cookie = "iconSize=" + 250 + "; SameSite=None; Secure";
                document.cookie = "defaultSliderValue=" + 1 + "; SameSite=None; Secure";
                break;
            case 2:
                setIconSize(300);
                document.cookie = "iconSize=" + 300 + "; SameSite=None; Secure";
                document.cookie = "defaultSliderValue=" + 2 + "; SameSite=None; Secure";
                break;
            case 3:
                setIconSize(350);
                document.cookie = "iconSize=" + 350 + "; SameSite=None; Secure";
                document.cookie = "defaultSliderValue=" + 3 + "; SameSite=None; Secure";
                break;
            case 4:
                setIconSize(400);
                document.cookie = "iconSize=" + 400 + "; SameSite=None; Secure";
                document.cookie = "defaultSliderValue=" + 4 + "; SameSite=None; Secure";
                break;
            default:
                setIconSize(300);
                document.cookie = "iconSize=" + 300 + "; SameSite=None; Secure";
                document.cookie = "defaultSliderValue=" + 2 + "; SameSite=None; Secure";
        }
    }


    return (
        <div className={classes.root}>
            <AppBar position="static">
                <Toolbar>
                    <div className={classes.iconSizeSlider}>
                        <Slider defaultValue={defaultSliderValue}
                                aria-labelledby="discrete-slider"
                                valueLabelDisplay="auto"
                                step={1}
                                min={1}
                                max={4}
                                onChange={changeIconSize}
                        />
                    </div>
                </Toolbar>
            </AppBar>
            <GridList cellHeight={300}>
                {streamers.map(streamer => (<StreamersRows key={streamer.id} passedStreamer={streamer} iconSize={iconSize} />))}
            </GridList>
        </div>
    )
}