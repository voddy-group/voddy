import React, {useEffect, useState} from "react";
import RenderSearchRow from "./RenderSearchRow";
import "../../assets/styles/StreamSearch.css";
import SearchBar from "./SearchBar";
import {CircularProgress, makeStyles, Typography} from "@material-ui/core";

const styles = makeStyles((theme) => ({
    loading: {
        width: "100%",
        height: "100%",
        backgroundColor: "white",
        display: "flex",
        justifyContent: "center"
    }
}));

export default function Search() {
    const [searchValue, setSearchValue] = useState("");
    const [searchData, setSearchData] = useState([]);
    const [hideSuggested, setHideSuggested] = useState(false);
    const [followedChannels, setFollowedChannels] = useState([]);
    const classes = styles();

    useEffect(() => {
        getFollowedChannels();
    }, [])
    
    useEffect(() => {
        if (!searchValue) {
            setHideSuggested(false);
            setSearchData(followedChannels);
        }
    }, [searchValue])
    
    useEffect(() => {
        const delayDebounceFn = setTimeout(() => getSearch(), 1500)
        return () => clearTimeout(delayDebounceFn)
    }, [searchValue])

    async function getFollowedChannels() {
        var response = await fetch('twitchApi/followed',
            {
                method: 'get',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        
        var returnedData = await response.json();
        
        setFollowedChannels(returnedData);
        setSearchData(returnedData);
    }
    
    async function getSearch() {
        if (searchValue !== void (0) && searchValue !== "") {
            setSearchData([]);
            setHideSuggested(true);
            const response = await fetch('twitchApi/stream/search' +
                '?term=' + searchValue, {
                Method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const returnedData = await response.json();

            setHideSuggested(true);

            var streams = await getStreamers();
            
            // TODO make this efficient
            /*for (var x = 0; x < returnedData.data.length; x++) {
                for (var i = 0; i < streams.data.length; i++) {
                    if (returnedData.data[x].id === streams.data[i].streamerId) {
                        returnedData.data[x].alreadyAdded = true;
                        break;
                    } else {
                        returnedData.data[x].alreadyAdded = false;
                    }
                }
            }*/
            
            setSearchData(returnedData);
        }
    }

    async function getStreamers() {
        const response = await fetch('database/streamers',
            {
                Method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        
        return await response.json();
    }

    return (
        <div style={{width: "100%"}}>
            <SearchBar searchValue={setSearchValue} />
            <Typography hidden={hideSuggested} variant={"h2"}>Followed channels:</Typography>
            {searchData.length > 0 ?
                <div style={{marginLeft: "auto"}}>
                    {searchData.map(searchedData => <RenderSearchRow key={searchedData.streamerId}
                                                                     searchedData={searchedData}/>)}
                </div>
                :
                <div className={classes.loading}>
                    <CircularProgress/>
                </div>
            }
        </div>
    )
}