import React, {useState, useEffect} from "react";
import {GridListTile, GridListTileBar, makeStyles} from "@material-ui/core";
import {Link} from "react-router-dom";

const styles = makeStyles((theme) => ({
    GridListTile: {
        //width: 300,
        //height: 300,
        padding: 2
    },
    topTileBar: {
        backgroundColor: "unset"
    }
}));

export default function StreamersRows(props) {
    const [streamer] = useState(props.passedStreamer);
    const [streamerUrl] = useState("/streamer/" + streamer.id);
    const [isLive, setIsLive] = useState(props.passedStreamer.isLive);
    const classes = styles();

    props.hubConnection.on(props.passedStreamer.id + "Live", (message) => {
        if (message != null && message) {
            setIsLive(true);
        } else if (!message) {
            setIsLive(false);
        }
    })

    return (
        <GridListTile cols={2} className={classes.GridListTile} key={props.key}>
            <Link style={{ display: "block"}} to={streamerUrl}>
                <GridListTileBar className={classes.topTileBar} titlePosition={"top"} title={isLive ? "🔴 LIVE": null} />
                <img alt="thumbnail" style={{ width: props.iconSize, height: props.iconSize}} src={streamer.thumbnailLocation}/>
                <GridListTileBar title={streamer.displayName} />
            </Link>
        </GridListTile>
    )
}