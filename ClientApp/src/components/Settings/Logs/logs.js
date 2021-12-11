import React, {useEffect, useState} from "react";
import {
    IconButton, MenuItem,
    Paper, Select,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableFooter,
    TableHead,
    TablePagination,
    TableRow, Toolbar, Typography
} from "@material-ui/core";
import {FilterList} from "@material-ui/icons";

const columns = [
    {id: "logged", label: "Logged", maxWidth: 100},
    {id: "level", label: "Level", maxWidth: 50},
    {id: "message", label: "Message", maxWidth: 400},
    {id: "callsite", label: "Callsite", maxWidth: 400},
    {id: "exception", label: "Exception", maxWidth: 400}
]

export default function Logs() {
    const [logs, setLogs] = useState([]);
    const [count, setCount] = useState(0);
    const [page, setPage] = useState(0);
    const [rowsPerPage, setRowsPerPage] = useState(100);
    const [filter, setFilter] = useState("All");

    useEffect(() => {
        getLogs(0, rowsPerPage);
    }, [])

    async function getLogs(pageOffset, pageSize, filter) {
        let requestUrl = 'logs?pageOffset=' + pageOffset + '&pageSize=' + pageSize;
        if (filter != null) {
            requestUrl += "&levelFilter=" + filter
        }

        const request = await fetch(requestUrl, {
            method: 'get',
            headers: {
                'Content-Type': 'application/json'
            }
        });

        if (request.ok) {
            var response = await request.json();
            setLogs(response.logs);
            setCount(response.count);
        }
    }

    function handlePageChange(event, page) {
        getLogs(page * 100, rowsPerPage, filter);
        setPage(page);
    }

    function handleRowsPerPageChange(event) {
        setRowsPerPage(event.target.value);
        getLogs(page, event.target.value);
    }

    function handleFilterChange(event) {
        if (event.target.value !== "All") {
            getLogs(0, rowsPerPage, event.target.value);
        } else {
            getLogs(0, rowsPerPage);
        }
        setFilter(event.target.value);
    }

    return (
        <div>
            <Typography variant={"h2"}>Logs</Typography>
            <Typography variant={"body1"}>Application logs.</Typography>
            <Paper style={{width: "90%"}}>
                <Toolbar>
                    <Select
                        value={filter}
                        onChange={handleFilterChange}
                        defaultValue={null}>
                        <MenuItem value={"All"}>All</MenuItem>
                        <MenuItem value={"Error"}>Errors Only</MenuItem>
                        <MenuItem value={"Warn"}>Warnings Only</MenuItem>
                    </Select>
                </Toolbar>
                <TableContainer style={{maxHeight: 500}}>
                    <Table stickyHeader>
                        <TableHead>
                            <TableRow>
                                <TableCell>
                                    Logged
                                </TableCell>
                                <TableCell>
                                    Level
                                </TableCell>
                                <TableCell>
                                    Message
                                </TableCell>
                                <TableCell>
                                    Callsite
                                </TableCell>
                                <TableCell>
                                    Exception
                                </TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {logs.map((log) => {
                                return (
                                    <TableRow key={log.id}>
                                        {columns.map((column) => {
                                            const value = log[column.id];
                                            return (
                                                <TableCell key={column.id} style={{
                                                    maxWidth: column.maxWidth,
                                                    overflow: "hidden",
                                                    whiteSpace: "nowrap"
                                                }}>
                                                    {value}
                                                </TableCell>
                                            )
                                        })}
                                    </TableRow>
                                )
                            })}
                        </TableBody>
                    </Table>
                </TableContainer>
                <TablePagination
                    component={"div"}
                    count={count}
                    onChangePage={handlePageChange}
                    page={page}
                    rowsPerPage={100}
                    onChangeRowsPerPage={handleRowsPerPageChange}/>
            </Paper>
        </div>
    )
}