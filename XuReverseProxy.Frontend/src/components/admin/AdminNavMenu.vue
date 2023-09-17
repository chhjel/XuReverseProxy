<script lang="ts">
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import { Options } from "vue-class-component";
import { Inject, Vue } from 'vue-property-decorator'

@Options({
	components: {
	}
})
export default class AdminNavMenu extends Vue {
  	@Inject()
	readonly options!: AdminPageFrontendModel;
	
	async mounted() {
	}
}
</script>

<template>
	<div>
		<div class="logo-text">{{ (options.serverName || 'XuReverseProxy') }}</div>

		<nav class="admin-nav-desktop">
			<router-link to="/proxyconfigs">Proxies</router-link> |
			<router-link to="/clients">Clients</router-link> |
			<router-link to="/notifications">Notifications</router-link> |
			<router-link to="/serverconfig">Server config</router-link> |
			<router-link to="/jobs">Jobs</router-link> |
			<router-link to="/admin-audit-log">Admin log</router-link> |
			<router-link to="/client-audit-log">Client log</router-link> |
			<router-link to="/server-log">Server log</router-link>
			<div class="spacer"></div>
			<a href="/auth/logout" class="logout">[Logout]</a>
		</nav>

		<nav class="admin-nav-mobile">
			<router-link to="/proxyconfigs">Proxies</router-link>
			<router-link to="/clients">Clients</router-link>
			<router-link to="/notifications">Notifications</router-link>
			<router-link to="/serverconfig">Server config</router-link>
			<router-link to="/jobs">Jobs</router-link>
			<router-link to="/admin-audit-log">Admin log</router-link>
			<router-link to="/client-audit-log">Client log</router-link>
			<router-link to="/server-log">Server log</router-link>
			<a href="/auth/logout" class="logout">Logout</a>
		</nav>
	</div>
</template>

<style scoped lang="scss">
.logo-text {
	font-size: 48px;
	font-weight: 600;
	margin-bottom: 2px;
	margin-left: -5px;
	user-select: none;
  
	@media (max-width: 869px) {
		font-size: 24px;
		margin-left: 0;
		margin-bottom: 16px;
	}
}

.admin-nav-desktop {
	display: flex;
	overflow-y: auto;
	padding-bottom: 8px;
	border-bottom: 2px solid var(--color--secondary-darken);
	@media (max-width: 869px) { display: none; }

	a {
		margin-right: 5px;
		white-space: nowrap;

		&:not(:first-child) {
			margin-left: 5px;
		}
	}

    .router-link-active {
		text-decoration: underline;
	}

	.logout {
		color: var(--color--danger);
	}
}

.admin-nav-mobile {
	display: flex;
	overflow-y: auto;
	padding-bottom: 10px;
	border-bottom: 3px solid var(--color--secondary-darken);
	@media (min-width: 870px) { display: none; }

	a {
		margin-right: 5px;
		white-space: nowrap;
		padding: 20px 10px;
		border: 1px solid var(--color--secondary);

		&:not(:first-child) {
			margin-left: 5px;
		}
	}

    .router-link-active {
		background-color: var(--color--secondary-darken);
  		border-color: var(--color--secondary-darken);
		text-decoration: underline;
	}

	.logout {
		color: var(--color--danger-lighten);
		background-color: #240d0d;
		border-color: #4d1e1e;
	}
}
</style>
