import React, {useEffect, useState} from "react";
import {fade, makeStyles, TextField} from "@material-ui/core";
import {Autocomplete} from "@material-ui/lab";
import {createBrowserHistory} from 'history';
import {Link} from "react-router-dom";

const styles = makeStyles((theme) => ({
    search: {
        position: "relative",
        marginLeft: 0,
        borderRadius: theme.shape.borderRadius,
        backgroundColor: fade(theme.palette.common.white, 0.15),
        '&:hover': {
            backgroundColor: fade(theme.palette.common.white, 0.25),
        },
        [theme.breakpoints.up('sm')]: {
            marginLeft: theme.spacing(1),
            width: 300,
        },
    },
    inputRoot: {
        color: "inherit"
    },
    inputInterior: {
        padding: theme.spacing(1, 1, 1, 0),
        paddingLeft: 'calc(1em + ${theme.spacing(4)}px',
        transition: theme.transitions.create('width'),
        width: "100%",
        [theme.breakpoints.up('sm')]: {
            width: '12ch',
            '&:focus': {
                width: '20ch',
            },
        }
    }
}));

export default function TopBarSearch() {
    const [open, setOpen] = useState(false);
    const [streamers, setStreamers] = useState([]);
    const classes = styles();
    const history = createBrowserHistory();

    useEffect(() => {
        getStreamers();
    }, [])

    async function getStreamers() {
        const request = await fetch('streamer/list', {
            method: 'get',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (request.status === 200) {
            var response = await request.json();

            setStreamers(response.data);
        }
    }

    function handleStreamerClick(streamer) {
        if (streamer !== null)
            history.push("/streamer/" + streamer.id);
    }

    return (
        <div>
            <Autocomplete
                clearOnBlur
                style={{width: 300}}
                open={open}
                onOpen={() => {
                    setOpen(true);
                }}
                onClose={() => {
                    setOpen(false);
                }}
                getOptionSelected={(option, value) => option.displayName === value.displayName}
                getOptionLabel={(option) => option.displayName}
                renderOption={(option) => (
                    <div>
                        <img style={{height: 20, paddingRight: 10}} src={option.thumbnailLocation} />
                        <Link to={"/streamer/" + option.id}>{option.displayName}</Link>
                    </div>
                )}
                renderInput={(params) => (
                    <TextField {...params} label={"Search"}/>
                )}
                options={streamers}
            />
        </div>
    )
}