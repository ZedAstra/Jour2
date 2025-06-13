<script lang="ts">
	import "./app.css";
	import avatar from "./assets/avatar.png";
	import { Bell, Eye, EyeOff, ArrowLeftRight, ScanQrCode, ChevronRight } from "lucide-svelte";

	let balance = $state(120000);
	let currency = $state("F");
	let balanceVisible = $state(true);


	class Transaction {
		constructor(
			public destinataire: string,
			public heure: string,
			public montant: number,
			public sujet: string,
		) {}
	}

	let transactions: Transaction[] = $state([
		new Transaction("À vous même", "18:38", 205_550, "Pension"),
	]);
</script>

<main class="flex flex-col gap-6">
	<!-- Header -->
	<header class="w-full h-16 bg-amber-50 flex flex-row">
		<div class="flex-1 flex items-center justify-center">
			<h1 class="text-2xl font-bold text-amber-900">Svelte</h1>
		</div>
		<div class="flex-1"></div>
		<div class="flex flex-row items-center justify-end p-4 gap-4">
			<button class="rounded-full bg-white size-12 items-center justify-center" aria-label="Avatar" onclick={()=>alert('Avatar clicked!')}>
				<img src={avatar} class="size-full rounded-full" alt="avatar"/>
			</button>
			<button class="rounded-full bg-white size-12 ">
				<Bell size=24 class="m-auto"/>
			</button>
		</div>
	</header>
	<!-- Main Content -->
	<div class="flex flex-col h-48 bg-teal-800 text-white rounded-lg mx-8">
		<!-- Solde -->
		<div class="flex-1 border-b-2 border-dotted border-[#ffffff44] flex flex-col items-center justify-center">
			<p class="text-sm text-[#ffffffBB]">Solde principal</p>
			<button onclick={() => balanceVisible = !balanceVisible} class="inline-flex items-center gap-2 text-white">
				{#if balanceVisible}
					<EyeOff size={24} />
				{:else}
					<Eye size={24} />
				{/if}
				<span class="text-3xl font-bold">
					{balanceVisible ? balance.toLocaleString() + " " + currency : "*****"}
					
				</span>
			</button>
		</div>
		<!-- Actions -->
		<div class="flex-1 flex flex-row items-center justify-center gap-4">
			<button class="flex flex-col">
				<div class="flex bg-[#00000066] size-12 rounded-full">
					<ArrowLeftRight size={24} class="m-auto" />
				</div>
				<p class="text-xs text-center">Tranférer</p>
			</button>
			<button class="flex flex-col">
				<div class="flex bg-[#00000066] size-12 rounded-full">
					<ScanQrCode size={24} class="m-auto" />
				</div>
				<p class="text-xs text-center">Scanner</p>
			</button>
		</div>
	</div>
	<!-- Fonctions -->
	<div class="flex flex-row overflow-x-scroll h-26 gap-4 mx-8">
		<button class="flex flex-col min-w-18 h-full size-16 items-center" aria-label="Cotisations">
			<div class="flex bg-amber-100 rounded-full size-14">

			</div>
			<p class="text-xs text-center mt-1">Cotisations</p>
		</button>
		<button class="flex flex-col min-w-18 h-full size-16 items-center" aria-label="Demander un prêt">
			<div class="flex bg-amber-100 rounded-full size-14">

			</div>
			<p class="text-xs text-center mt-1">Demander un prêt</p>
		</button>
		<button class="flex flex-col min-w-18 h-full size-16 items-center" aria-label="Coffres">
			<div class="flex bg-amber-100 rounded-full size-14">

			</div>
			<p class="text-xs text-center mt-1">Coffres</p>
		</button>
		<button class="flex flex-col min-w-18 h-full size-16 items-center" aria-label="Effectuer un paiement">
			<div class="flex bg-amber-100 rounded-full size-14">

			</div>
			<p class="text-xs text-center mt-1">Effectuer un paiement</p>
		</button>
		<button class="flex flex-col w-18 h-full size-16 items-center" aria-label="Épargne">
			<div class="flex bg-amber-100 rounded-full size-14">

			</div>
			<p class="text-xs text-center mt-1">Épargne</p>
		</button>
	</div>
	<!-- Carte? -->
	<div class="flex flex-col h-38 mx-8 bg-green-50 p-2">
		<div>
			<h2 class="font-semibold m-4">Incroyable Tout à 1% 🤩</h2>
			<p class="text-xs ml-12">Nous avons le plaisir de vous annoncer la mise 
				en place d'un bonus frais de 1% applicable sur
				vos opérations.</p>
		</div>
		<div class="flex flex-row items-center justify-center gap-2 m-4">
			<div class="h-1 w-4 bg-orange-400 rounded-xs"></div>
			<div class="h-1 w-4 bg-white rounded-xs"></div>
			<div class="h-1 w-4 bg-white rounded-xs"></div>
			<div class="h-1 w-4 bg-white rounded-xs"></div>
		</div>
	</div>

	<div class="flex flex-row justify-between mx-8">
		<p class="font-semibold">Transactions</p>
		<button class="text-sm text-orange-400">Tout afficher</button>
	</div>

	<!-- Transactions -->
	<div class="flex flex-col gap-2 mx-8">
	<p class="text-sm text-[#00000066]">Aujourd'hui</p>
	{#each transactions as transaction}
		<div class="flex flex-row h-16 items-center">
			<div class="size-12 rounded-full bg-amber-100"></div>
			<div class="flex-1 flex flex-col ml-4">
				<p class="text-sm font-semibold">{transaction.destinataire}</p>
				<p class="text-xs text-green-600 font-bold">{transaction.sujet}</p>
				<p class="text-xs text-[#00000066]">{transaction.heure}</p>
			</div>
			<div class="flex flex-1 justify-end w-24">
				<p class="text-lg font-bold">
					{transaction.montant.toLocaleString()} {currency}
				</p>	
			</div>
			<div class="flex flex-col items-center justify-center ml-2">
				<button class="flex rounded-full bg-orange-100 text-orange-400 size-6 items-center justify-center p-auto" aria-label="Details" onclick={()=>alert('Details clicked!')}>
					<ChevronRight size={16} />
				</button>

			</div>	
		</div>
	{/each}
	</div>
</main>

<style>
	button {
		transition: transform 0.1s ease-in-out;
	}
	button:active {
		transform: scale(0.95);
	}
</style>
