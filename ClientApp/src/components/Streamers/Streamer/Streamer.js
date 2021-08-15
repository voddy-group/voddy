import React, {useState, useEffect} from "react";
import {useHistory} from "react-router-dom";
import StreamerStreams from "./StreamerStreams";
import "../../../assets/styles/StreamSearch.css";
import {
    AppBar,
    BottomNavigation,
    BottomNavigationAction,
    Button,
    CircularProgress, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle,
    GridList,
    IconButton,
    makeStyles, Snackbar,
    SvgIcon,
    Toolbar,
    Typography
} from "@material-ui/core";
import StreamerDownloadAll from "./StreamerDownloadAll";
import StreamerSettings from "./Settings/StreamerSettings";
import {Alert, Pagination} from "@material-ui/lab";
import {HubConnection} from "@microsoft/signalr";

const styles = makeStyles((theme) => ({
    root: {
        display: "flex",
        flexWrap: "wrap",
        overflow: "hidden",
        width: "100%"
    },
    grow: {
        flexGrow: 1
    },
    appbar: {
        backgroundColor: "unset",
        paddingBottom: 50,
        paddingTop: 50
    },
    appbarImg: {
        height: 150,
        position: "relative",
        paddingRight: 50
    },
    loading: {
        width: "100%",
        height: "100%",
        display: "flex",
        justifyContent: "center"
    },
    appbarData: {
        maxWidth: "80%",
        height: "100%",
        display: "flex",
        flexDirection: "column"
    },
    noStreams: {
        width: "100%",
        textAlign: "center"
    },
    flexGrow: {
        flexGrow: 1
    },
    centreNav: {
        display: "flex",
        borderRadius: "5px 5px 0 0",
        backgroundColor: "darkgrey",
        overflow: "hidden"
    },
    bottomNav: {
        width: "80%",
        position: "fixed",
        bottom: 0,
        height: 50,
        backgroundColor: "unset"
    }
}));

export default function Streamer(passed) {
    const [streamer, setStreamer] = useState({});
    const [streams, setStreams] = useState([]);
    const [streamPage, setStreamPage] = useState([]);
    const [size, setSize] = useState(0);
    const [noStreams, setNoStreams] = useState(false);
    const [showPaging, setShowPaging] = useState(false);
    const [loading, setLoading] = useState(true);
    const [deleteWarningOpen, setDeleteWarningOpen] = useState(false);
    const classes = styles();
    //let page = 0;
    let history = useHistory();

    useEffect(() => {
        GetStreamer();
    }, [passed.id])

    async function GetStreamer() {
        const request = await fetch('database/streamers' +
            '?id=' + passed.id,
            {
                Method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        var response = await request.json();

        GetStreamerMetadata(response.data[0].streamerId);
        setStreamer(response.data[0]);
        GetStreamerStreams(response.data[0].streamerId);
    }

    async function GetStreamerMetadata(id) {
        const request = await fetch('database/streamerMeta' +
            '?streamerId=' + id,
            {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        var response = await request.json();

        setSize(response.size);
    }

    async function GetStreamerStreams(id) {
        const request = await fetch('streams/getStreamsWithFilter' +
            '?id=' + id,
            {
                Method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

        var response = await request.json();

        if (response.length > 0) {
            if (response.length > 30) {
                var pages = [];
                for (var x = 0; x < response.length; x += 30) {
                    pages.push(response.slice(x, x + 30));
                }
                setStreams(pages)
                setLoading(false);
                setStreamPage(pages[0]);
                setShowPaging(true);
            } else {
                setStreamPage(response);
            }
        } else {
            setNoStreams(true);
        }
    }

    function setAdded(added) {
        var newStreams = [...streams];
        for (var x = 0; x < newStreams.length; x++) {
            newStreams[x].alreadyAdded = added;
        }
        return newStreams;
    }

    async function DeleteStreamer() {
        const request = await fetch('database/streamer' +
            '?streamerId=' + streamer.streamerId,
            {
                method: 'delete',
                headers: {
                    'Content-Type': 'application/json'
                }
            })

        if (request.ok) {
            history.push({
                pathname: "/",
                state: {
                    notification: true,
                    notifMessage: "Streamer deleted.",
                    notifSeverity: "info"
                }
            });
        }
    }

    function calculateSize() {
        // 71811754
        if (size > 1000000) {
            //mb
            if (size > 1000000000) {
                //gb
                return (size / 1000000000).toFixed(2) + " GB";
            }
            return (size / 1000000).toFixed(2) + " MB";
        } else {
            return "0 MB"
        }
    }

    function streamRender() {
        if (streamPage.length > 0 || !loading) {
            return <GridList cellHeight={180} style={{paddingBottom: 50}}>
                {streamPage.map(stream => <StreamerStreams key={stream.streamId} passedStream={stream} hubConnection={passed.hubConnection} />)}
            </GridList>;

        }
        if (noStreams) {
            return <div className={classes.noStreams}>
                <Typography>No streams!</Typography>
            </div>
        }
        return <div className={classes.loading}>
            <CircularProgress/>
        </div>
    }

    function changePage(event, pageNumber) {
        setStreamPage(streams[pageNumber - 1]);
    }
    
    function handleDeleteWarningClose() {
        setDeleteWarningOpen(false);
    }
    
    function handleDeleteButton() {
        setDeleteWarningOpen(true);
    }

    // TODO needs performance checks; relies on other calls too much

    return (
        <div className={classes.root}>
            <AppBar className={classes.appbar} position="static" elevation={0} style={{backgroundColor: "unset"}}>
                <Toolbar>
                    <div style={{position: "relative"}}>
                        <img className={classes.appbarImg} src={streamer.thumbnailLocation}/>
                        <Typography style={{
                            position: "absolute",
                            top: 5,
                            left: 5
                        }}>{streamer.isLive ? '🔴 LIVE' : null}</Typography>
                    </div>
                    <div className={classes.appbarData}>
                        <Typography variant={"h3"}>{streamer.displayName}</Typography>
                        <div style={{marginTop: "auto"}}>
                            <Typography style={{display: "inline-block"}} variant={"subtitle1"}>
                                <SvgIcon
                                    style={{display: "inline-block"}}>
                                    <path fill="black"
                                          d="M13,9H11V7H13M13,17H11V11H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z"/>
                                </SvgIcon>
                                {streamer.description}</Typography>
                            <div>
                                <Typography style={{display: "inline-block"}}
                                            variant={"subtitle1"}>
                                    <SvgIcon>
                                        <path fill="black"
                                              d="M12,3C7.58,3 4,4.79 4,7C4,9.21 7.58,11 12,11C16.42,11 20,9.21 20,7C20,4.79 16.42,3 12,3M4,9V12C4,14.21 7.58,16 12,16C16.42,16 20,14.21 20,12V9C20,11.21 16.42,13 12,13C7.58,13 4,11.21 4,9M4,14V17C4,19.21 7.58,21 12,21C16.42,21 20,19.21 20,17V14C20,16.21 16.42,18 12,18C7.58,18 4,16.21 4,14Z"/>
                                    </SvgIcon>
                                    {calculateSize()}</Typography>
                            </div>
                            <div>
                                <Typography style={{display: "inline-block"}} variant={"subtitle1"}>
                                    <SvgIcon>
                                        <path fill="black"
                                              d="M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17M12,4.5C7,4.5 2.73,7.61 1,12C2.73,16.39 7,19.5 12,19.5C17,19.5 21.27,16.39 23,12C21.27,7.61 17,4.5 12,4.5Z"/>
                                    </SvgIcon>{streamer.viewCount ? streamer.viewCount.toLocaleString() : streamer.viewCount} Views</Typography>
                            </div>
                        </div>
                    </div>
                    <div className={classes.grow}/>
                    <div>
                        <StreamerDownloadAll streams={streams} streamerId={passed.id} setStreams={setStreams}/>
                        <StreamerSettings streamer={streamer}/>
                        <IconButton onClick={handleDeleteButton}>
                            <SvgIcon>
                                <path fill="black"
                                      d="M19,4H15.5L14.5,3H9.5L8.5,4H5V6H19M6,19A2,2 0 0,0 8,21H16A2,2 0 0,0 18,19V7H6V19Z"/>
                            </SvgIcon>
                        </IconButton>
                        <Dialog open={deleteWarningOpen} onClose={setDeleteWarningOpen}>
                            <DialogTitle>Delete Streamer?</DialogTitle>
                            <DialogContent>
                                <DialogContentText>
                                    Delete the streamer from the database.
                                </DialogContentText>
                                <DialogContentText style={{color: "red"}}>
                                    Warning: This will delete all VODs of the streamer.
                                </DialogContentText>
                            </DialogContent>
                            <DialogActions>
                                <Button onClick={handleDeleteWarningClose} color={"primary"} autoFocus>
                                    Cancel
                                </Button>
                                <Button onClick={DeleteStreamer} color={"primary"}>
                                    Delete
                                </Button>
                            </DialogActions>
                        </Dialog>
                    </div>
                </Toolbar>
            </AppBar>
            {streamRender()}
            {showPaging ?
                <div style={{display: "flex", width: "100%"}}>
                    <div className={classes.flexGrow}/>
                    <Pagination count={streams.length} color={"primary"} onChange={changePage} showFirstButton
                                showLastButton/>
                    <div className={classes.flexGrow}/>
                </div>
                :
                null
            }
        </div>
    )
}