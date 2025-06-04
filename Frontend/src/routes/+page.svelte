<script lang="ts">
    import { fly, fade } from "svelte/transition";
    import * as api from "$lib/api";
	import { onMount } from "svelte";

    let entries: string[] = $state([]);
    let selectedEntry: string | null = $state(null);
    let entryContent: string[][] = $state([]);
    let error: string | null = $state(null);

    $effect(() => {
        showToast(error);
    });
    
    let refreshing: boolean = $state(false);
    async function refresh() {
        refreshing = true;
        const data = await api.getEntries();
        if(data) {
            selectedEntry = null;
            entryContent = [];
            entries = data;
        }
        refreshing = false;
    }
    let loadingEntry: boolean = $state(false);
    async function fetchEntry(name: string) {
        loadingEntry = true;
        const data = await api.getEntry(name);
        if(data) {
            selectedEntry = name;
            entryContent = data;
        } else {
            selectedEntry = null;
            entryContent = [];
        }
        loadingEntry = false;
    }
    let loadingWholePage: boolean = $state(false);
    async function submitFile() {
        const file = ((window as any).fileInput as HTMLInputElement).files?.[0];
        if(!file) {
            showToast("Please select a file to upload.", "error");
            return;
        }
        ((window as any).fileInput as HTMLInputElement).value = "";
        ((window as any).addNewDetails as HTMLDetailsElement).open = false;
        loadingWholePage = true;
        const data = await api.uploadEntry(file);
        loadingWholePage = false;
        if(!data) {
            error = "Failed to upload the file. Please try again.";
            showToast(error, "error");
        } else {
            showToast("File uploaded successfully!", "success");
            await refresh();
        }
    }

    let toastVisible: boolean = $state(false);
    let toastMessage: string | null = $state(null);
    let toastType: "success" | "neutral" | "error" = $state("neutral");
    function showToast(message: string | null, type: "success" | "neutral" | "error" = "neutral") {
        if (message) {
            toastMessage = message;
            toastType = type;
            toastVisible = true;
            setTimeout(() => {
                toastMessage = null;
                toastVisible = false;
                error = null; // Clear the error after showing the toast
            }, 3000); // Hide after 3 seconds
        }
    }

    onMount(() => {
        refresh();
    });
    
</script>

<div class="w-screen">
    <div class="toast toast-end">
        {#if toastVisible && toastType === "success"}
            <div class="alert alert-success" transition:fly={{ duration: 300, x: 64}}>
                <span>{toastMessage}</span>
            </div>
        {:else if toastVisible && toastType === "neutral"}
            <div class="alert alert-info" transition:fly={{ duration: 300, x: 64}}>
                <span>{toastMessage}</span>
            </div>
        {:else if toastVisible && toastType === "error"}
            <div class="alert alert-error" transition:fly={{ duration: 300, x: 64}}>
                <span>{toastMessage}</span>
            </div>
        {/if}
    </div>
    <div class="flex w-screen h-screen flex-row">
        <div class="flex flex-col p-2 w-96 h-full bg-neutral-200">
            <div class="join join-horizontal">
                <button class="btn btn-primary {(refreshing ? "btn-disabled" : "")} w-1/2 join-item" onclick={refresh}>
                    Refresh
                    {#if refreshing}
                        <span class="loading loading-spinner loading-sm"></span>
                    {/if}
                </button>
                <details id="addNewDetails" class="dropdown w-1/2">
                    <summary class="btn btn-success w-full join-item">Add Entry +</summary>
                    <ul class="menu dropdown-content bg-base-100 rounded-box z-1 w-80 p-2 shadow-sm">
                        <input id="fileInput" type="file" class="file-input">
                        <button class="btn btn-secondary w-full" onclick={submitFile}>Submit</button>
                    </ul>
                </details>
            </div>
            <div class="flex-1 overflow-y-scroll p-2 gap-2">
                {#if entries.length == 0}
                    <div class="alert alert-warning">
                        <span>No entries found. Click refresh to load data.</span>
                    </div>
                {/if}
                {#each entries as entry}
                    <button class="btn btn-info w-full {selectedEntry === entry ? "btn-disabled" : ""}" onclick={() => fetchEntry(entry)}>
                        {entry}
                    </button>
                {/each}
            </div>
        </div>
        <div class="flex flex-1 p-2 bg-neutral-300 overflow-x-auto">
            {#if entryContent.length > 0}
                <table class="table w-full h-fit">
                    {#each entryContent as row, index}
                        {#if index === 0}
                            <thead>
                                <tr>
                                    {#each row as header}
                                        <th class="p-2 text-left">{header}</th>
                                    {/each}
                                </tr>
                            </thead>
                        {:else}
                            <tbody>
                                <tr>
                                    {#each row as cell}
                                        <td class="p-2">
                                            {cell}
                                        </td>
                                    {/each}
                                </tr>
                            </tbody>
                        {/if}
                    {/each}
                </table>
            {:else}
                ?
            {/if}
        </div>
    </div>
    {#if loadingWholePage}
        <div class="fixed inset-0 flex items-center justify-center bg-neutral-100 bg-opacity-50 z-50">
            <span class="loading loading-spinner loading-lg"></span>
        </div>
    {/if}
</div>