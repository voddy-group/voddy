import React, {useState, useEffect} from "react";
import {GridListTile, GridListTileBar, Link, makeStyles} from "@material-ui/core";

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

export default function StreamersRows(passedStreamer) {
    const [streamer] = useState(passedStreamer.passedStreamer);
    const [streamerUrl] = useState("/streamer/" + streamer.id);
    const classes = styles();

    return (
        <GridListTile cols={2} className={classes.GridListTile} key={passedStreamer.key}>
            <a style={{ display: "block" }} href={streamerUrl}>
                <GridListTileBar className={classes.topTileBar} titlePosition={"top"} title={streamer.isLive ? "🔴 LIVE": null} />
                <img alt="thumbnail" style={{ width: passedStreamer.iconSize, height: passedStreamer.iconSize}} src={streamer.thumbnailLocation}/>
                <GridListTileBar title={streamer.displayName} />
            </a>
        </GridListTile>
    )
}