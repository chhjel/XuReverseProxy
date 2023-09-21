<script lang="ts">
import { Options } from "vue-class-component";
import { Vue, Inject } from "vue-property-decorator";
import TextInputComponent from "@components/inputs/TextInputComponent.vue";
import ButtonComponent from "@components/inputs/ButtonComponent.vue";
import AdminNavMenu from "@components/admin/AdminNavMenu.vue";
import { AdminPageFrontendModel } from "@generated/Models/Web/AdminPageFrontendModel";
import LoaderComponent from "@components/common/LoaderComponent.vue";
import ClientAuditLogComponent from "@components/admin/ClientAuditLogComponent.vue";

@Options({
  components: {
    TextInputComponent,
    ButtonComponent,
    AdminNavMenu,
    LoaderComponent,
    ClientAuditLogComponent,
  },
})
export default class ClientAuditLogPage extends Vue {
  @Inject()
  readonly options!: AdminPageFrontendModel;

  async mounted() {}
}
</script>

<template>
  <div class="clientauditlog-page">
    <client-audit-log-component />
  </div>
</template>

<style scoped lang="scss">
.clientauditlog-page {
  padding-top: 20px;
}
</style>
