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

export default function StreamersRows(passedStreamer) {
    const [streamer] = useState(passedStreamer.passedStreamer);
    const [streamerUrl] = useState("/streamer/" + streamer.id);
    const classes = styles();

    return (
        <GridListTile cols={2} className={classes.GridListTile} key={passedStreamer.key}>
            <Link style={{ display: "block"}} to={streamerUrl}>
                <GridListTileBar className={classes.topTileBar} titlePosition={"top"} title={streamer.isLive ? "🔴 LIVE": null} />
                <img alt="thumbnail" style={{ width: passedStreamer.iconSize, height: passedStreamer.iconSize}} src={streamer.thumbnailLocation}/>
                <GridListTileBar title={streamer.displayName} />
            </Link>
        </GridListTile>
    )
}