<script lang="ts">
import { Options } from "vue-class-component";
import { Vue } from 'vue-property-decorator'

@Options({
	components: {
	}
})
export default class AdminNavMenu extends Vue {
	async mounted() {
	}
}
</script>

<template>
	<nav class="admin-nav">
		<!-- <router-link to="/">Overview</router-link> | -->
		<router-link to="/proxyconfigs">Proxies</router-link> |
		<router-link to="/clients">Clients</router-link> |
		<router-link to="/serverconfig">Server config</router-link> |
		<router-link to="/jobs">Jobs</router-link> |
		<router-link to="/admin-audit-log">Admin log</router-link> |
		<router-link to="/client-audit-log">Client log</router-link> |
		<a href="/auth/logout">[Logout]</a>
	</nav>
</template>

<style scoped lang="scss">
.admin-nav {
	overflow-y: auto;
    /* &__item {

    } */
}
</style>
